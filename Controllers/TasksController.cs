using AA_App.Data;
using AA_App.Models;
using AA_App.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AA_App.Controllers;

public class TasksController : Controller
{
    private readonly CollegeDbContext _db;
    public TasksController(CollegeDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var def = TaskCatalog.Find(id);
        if (def == null) return NotFound();
        return View(await BuildModel(def, def.Sample, null));
    }

    [HttpPost]
    public async Task<IActionResult> Run(int id)
    {
        var def = TaskCatalog.Find(id);
        if (def == null || def.Run == null) return NotFound();

        var cols = def.Columns;
        var titles = cols.ToDictionary(c => c.Key, c => c.Title);
        var values = cols.Select(c => Request.Form["c_" + c.Key].ToArray()).ToArray();
        var count = values.Length == 0 ? 0 : values.Max(v => v.Length);

        var texts = new List<string[]>();
        var rows = new List<Row>();
        for (var i = 0; i < count; i++)
        {
            var arr = new string[cols.Length];
            var dict = new Dictionary<string, string>();
            for (var k = 0; k < cols.Length; k++)
            {
                arr[k] = i < values[k].Length ? (values[k][i] ?? "") : "";
                dict[cols[k].Key] = arr[k];
            }
            if (arr.All(string.IsNullOrWhiteSpace)) continue;
            texts.Add(arr);
            rows.Add(new Row(dict, titles));
        }

        TaskResult result;
        if (rows.Count == 0)
        {
            result = new TaskResult();
            result.Err("Заполните хотя бы одну строку таблицы.");
        }
        else
        {
            try
            {
                Student.ResetIdCounter();
                result = def.Run(rows);
            }
            catch (Exception ex)
            {
                result = new TaskResult();
                result.Err("Ошибка выполнения: " + ex.Message);
            }

            _db.TaskRuns.Add(new TaskRun
            {
                TaskNumber = def.Number,
                TaskTitle = def.Title,
                RowsCount = rows.Count,
                Summary = result.Summary(),
                CreatedAt = DateTime.Now
            });
            await _db.SaveChangesAsync();
        }

        return View("Details", await BuildModel(def, texts.Count > 0 ? texts : def.Sample, result));
    }

    private async Task<TaskPageModel> BuildModel(TaskDef def, List<string[]> rows, TaskResult result)
    {
        var history = await _db.TaskRuns.Where(r => r.TaskNumber == def.Number)
                                        .OrderByDescending(r => r.Id).Take(5).ToListAsync();
        return new TaskPageModel { Def = def, Rows = rows, Result = result, History = history };
    }
}
