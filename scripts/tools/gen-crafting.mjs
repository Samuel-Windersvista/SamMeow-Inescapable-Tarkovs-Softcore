// gen-crafting.mjs — 从源 mod 的 TypeScript 资产生成 Softcore G6-D 制造数据 JSON。
//
// 源文件（只读）：
//   src/assets/productionAdjustments.ts  ->  data/softcore/crafting-rebalance.json
//   src/assets/recipes.ts                ->  data/softcore/crafting-recipes.json
//
// productionAdjustments.ts 的每条 adjust 是闭包，无法直接序列化。本脚本以「录制代理」
// 执行闭包，把配方级/需求级赋值录制成声明式 ops（见下方 OPS 词汇表），再落盘 JSON。
// 为使 find(...) 谓词命中，合成一份「超集」需求列表：1 条 Area + 文件中出现的每个
// ItemTpl.* 各 1 条 Item 需求（不改变录制结果的语义）。
//
// 用法（需 Node 18+；符号表由 scripts/tools/dump-spt-symbols.cs 生成）：
//   node scripts/tools/gen-crafting.mjs --symbols D:\Temp\spt-symbols.json \
//     --source "E:\Game\EFT_Offline\Life_in_Norvinsk_v0.3.2\mods\[5]经济与制造系统大修-Softcore - 已AI优化\user\mods\odt-softcore\src"
//
// OPS 词汇表（CraftingChangesChanger 解释）：
//   {op:"count", value}                              设置 craft.count
//   {op:"setAllCounts", value}                       遍历 requirements，has-count 者 count=value
//   {op:"setCount", templateId, value}               find(templateId) 后 count=value
//   {op:"replaceTemplate", from, to}                 find(templateId==from) 后 templateId=to
//   {op:"setAreaLevel", value}                       find(type=="Area") 后 requiredLevel=value
//   {op:"replaceRequirements", requirements:[...]}   整段替换 craft.requirements
//   {op:"pushRequirement", requirement:{...}}        追加一条需求

import fs from "node:fs";
import path from "node:path";
import url from "node:url";

function arg(name, fallback) {
  const i = process.argv.indexOf(`--${name}`);
  return i >= 0 && process.argv[i + 1] ? process.argv[i + 1] : fallback;
}

const scriptDir = path.dirname(url.fileURLToPath(import.meta.url));
const repoRoot = path.resolve(scriptDir, "..", "..");

const symbolsPath = arg("symbols", "D:\\Temp\\spt-symbols.json");
const sourceDir = arg("source");
const outDir = arg("out", path.join(repoRoot, "data", "softcore"));

if (!sourceDir) throw new Error("缺少 --source 源 src 目录");

const symbols = JSON.parse(fs.readFileSync(symbolsPath, "utf8"));
const ItemTpl = symbols.ItemTpl;
const BaseClasses = symbols.BaseClasses;

function deepCopy(v) {
  return JSON.parse(JSON.stringify(v));
}

function stripImportsAndTypes(text) {
  return text
    .replace(/^\s*import .*$/gm, "")
    .replace(/^\s*\/\/.*$/gm, "")
    .replace(/export const /g, "const ")
    .replace(/: IHideoutProduction\[\]/g, "")
    .replace(/: IHideoutProduction/g, "");
}

// ---- recipes.ts -> crafting-recipes.json ----

const recipesText = fs.readFileSync(path.join(sourceDir, "assets", "recipes.ts"), "utf8");
const recipesCode = stripImportsAndTypes(recipesText);
const recipesFn = new Function(
  "ItemTpl",
  "BaseClasses",
  `${recipesCode}\nreturn { additionalCraftingRecipes, containerRecipes };`
);
const { additionalCraftingRecipes, containerRecipes } = recipesFn(ItemTpl, BaseClasses);

// ---- productionAdjustments.ts -> crafting-rebalance.json ----

const adjText = fs.readFileSync(path.join(sourceDir, "assets", "productionAdjustments.ts"), "utf8");
const adjCode = stripImportsAndTypes(adjText);
const adjFn = new Function("ItemTpl", "BaseClasses", `${adjCode}\nreturn craftingAdjustments;`);
const adjustments = adjFn(ItemTpl, BaseClasses);

const refNames = [...new Set([...adjText.matchAll(/ItemTpl\.([A-Za-z0-9_]+)/g)].map((m) => m[1]))];
const missing = refNames.filter((n) => !(n in ItemTpl));
if (missing.length) {
  throw new Error(`MISSING SYMBOLS: ${missing.join(", ")}`);
}

const synth = [{ type: "Area", areaType: 10, requiredLevel: 1 }];
const seenTpl = new Set();
for (const name of refNames) {
  const hex = ItemTpl[name];
  if (seenTpl.has(hex)) continue;
  seenTpl.add(hex);
  synth.push({ templateId: hex, count: 1, type: "Item", isFunctional: false });
}

function dedupe(ops) {
  const out = [];
  const seen = new Set();
  for (const op of ops) {
    const key = JSON.stringify(op);
    if (op.op === "setAllCounts" && seen.has(key)) continue;
    seen.add(key);
    out.push(op);
  }
  return out;
}

function recordAdjust(adjust) {
  const ops = [];
  const reqs = synth.map((r) => ({ ...r }));
  const push = (op) => ops.push(op);

  const matchProxy = (r) =>
    new Proxy(r, {
      set(target, prop, value) {
        if (prop === "count") push({ op: "setCount", templateId: target.templateId, value });
        else if (prop === "templateId") push({ op: "replaceTemplate", from: target.templateId, to: value });
        else if (prop === "requiredLevel") push({ op: "setAreaLevel", value });
        else push({ op: "setField", field: prop, value });
        target[prop] = value;
        return true;
      },
    });

  const loopProxy = (r) =>
    new Proxy(r, {
      set(target, prop, value) {
        if (prop === "count") push({ op: "setAllCounts", value });
        else if (prop === "templateId") push({ op: "replaceTemplate", from: target.templateId, to: value });
        else if (prop === "requiredLevel") push({ op: "setAreaLevel", value });
        else push({ op: "setField", field: prop, value });
        target[prop] = value;
        return true;
      },
    });

  const arr = {
    find(pred) {
      for (const r of reqs) {
        if (pred(r)) return matchProxy(r);
      }
      return undefined;
    },
    push(x) {
      push({ op: "pushRequirement", requirement: deepCopy(x) });
      reqs.push(deepCopy(x));
      return reqs.length;
    },
    forEach(fn) {
      reqs.forEach((r, i) => fn(loopProxy(r), i, arr));
    },
    get length() {
      return reqs.length;
    },
    [Symbol.iterator]() {
      let i = 0;
      return {
        next: () => (i < reqs.length ? { value: loopProxy(reqs[i++]), done: false } : { value: undefined, done: true }),
      };
    },
  };

  const craft = new Proxy(
    { count: 1 },
    {
      get(target, prop) {
        if (prop === "requirements") return arr;
        return target[prop];
      },
      set(target, prop, value) {
        if (prop === "requirements") push({ op: "replaceRequirements", requirements: deepCopy(value) });
        else if (prop === "count") push({ op: "count", value });
        else push({ op: "setField", field: prop, value });
        target[prop] = value;
        return true;
      },
    }
  );

  adjust(craft);
  return dedupe(ops);
}

const recorded = [];
for (const adjustment of adjustments) {
  const id = adjustment.id?.toString();
  if (!id) throw new Error("adjustment 缺少可解析的 id");
  const ops = recordAdjust(adjustment.adjust);
  const unknown = ops.filter((op) => op.op === "setField");
  if (unknown.length) {
    throw new Error(`adjustment ${id} 出现未支持字段赋值：${JSON.stringify(unknown)}`);
  }
  recorded.push({ id, ops });
}

fs.mkdirSync(outDir, { recursive: true });
fs.writeFileSync(
  path.join(outDir, "crafting-rebalance.json"),
  JSON.stringify({ recipeAdjustments: recorded }, null, 2)
);
fs.writeFileSync(
  path.join(outDir, "crafting-recipes.json"),
  JSON.stringify({ additionalRecipes: additionalCraftingRecipes }, null, 2)
);

console.log(
  `crafting-rebalance.json: ${recorded.length} adjustments; ` +
    `crafting-recipes.json: ${additionalCraftingRecipes.length} additional ` +
    `(containerRecipes ${containerRecipes.length} 保留在 T08 ContainerRecipes.cs)`
);
