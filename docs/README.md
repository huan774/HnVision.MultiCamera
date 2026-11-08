# 技术文档规范（索引）

> 状态：骨架已建立，内容随对话增量补充
> 命名规则：**`<命名空间>.技术文档.md`**，一个根命名空间一份，子命名空间内容并入所属根命名空间文档。

## 1. 命名与组织规则

| 规则 | 说明 |
|---|---|
| 文件名 | `<根命名空间>.技术文档.md`（如 `MultiSerVIsion.Solution.Domain.技术文档.md`） |
| 粒度 | 按**根命名空间**建文档；子命名空间（如 `...Domain.Entities.Configs`）作为文档内小节 |
| 新增命名空间 | 新建同名技术文档，并在本索引登记 |
| 内容来源 | 对话中已确认的设计决策、约定、已知问题；未提到的不臆造 |

## 2. 统一文档骨架

每份命名空间技术文档包含固定六节：

1. **职责与边界** —— 该层做什么、不做什么
2. **目录与关键类型** —— 文件/类型清单
3. **依赖方向** —— 允许依赖谁、禁止依赖谁
4. **既有约定** —— 必须遵守的编码与设计约定（含踩坑记录）
5. **已知问题 / 待办** —— 未闭环项
6. **变更记录** —— 日期 + 变更摘要

## 3. 文档清单

| 命名空间 | 文档 | 状态 |
|---|---|---|
| `MultiSerVIsion.Solution.Domain` | `MultiSerVIsion.Solution.Domain.技术文档.md` | 骨架 |
| `MultiSerVIsion.Solution.Application` | `MultiSerVIsion.Solution.Application.技术文档.md` | 骨架 |
| `MultiSerVIsion.Solution.Infrastructure` | `MultiSerVIsion.Solution.Infrastructure.技术文档.md` | 骨架 |
| `MultiSerVIsion.Solution.Presentation` | `MultiSerVIsion.Solution.Presentation.技术文档.md` | 骨架 |
| `MultiSerVIsion.Solution.Shared` | `MultiSerVIsion.Solution.Shared.技术文档.md` | 骨架 |

## 4. 维护要求

- 设计与约定变更时，**先更新对应技术文档，再改代码**。
- 决策类变更需在文档「既有约定」或「变更记录」中标注原因与影响范围。
- 与算法配方页相关的设计基线见 `C:\Users\MR\Desktop\算法配方技术设计文档.docx`，本目录文档不重复其内容，仅登记引用关系。
