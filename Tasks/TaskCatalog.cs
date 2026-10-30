using System.Reflection;
using AA_App.Models;
using AA_App.Services;

namespace AA_App.Tasks;

public static class TaskCatalog
{
    public static IReadOnlyList<TaskDef> All { get; } = Build();

    public static TaskDef Find(int number) => All.FirstOrDefault(t => t.Number == number);

    // ---------- вспомогательные методы ----------
    static List<string[]> R(params string[][] rows) => rows.ToList();
    static string[] A(params string[] v) => v;

    static TaskDef D(int n, string section, string title, string desc, Column[] cols, List<string[]> sample, Func<List<Row>, TaskResult> run)
        => new TaskDef { Number = n, Section = section, Title = title, Description = desc, Columns = cols, Sample = sample, Run = run };

    static TaskResult Each(List<Row> rows, Action<Row, TaskResult> body)
    {
        var res = new TaskResult();
        var n = 0;
        foreach (var row in rows)
        {
            n++;
            try { body(row, res); }
            catch (Exception ex) when (ex is ArgumentException || ex is FormatException || ex is InvalidOperationException)
            {
                res.Err($"Строка {n}: {ex.Message}");
            }
        }
        return res;
    }

    static Column[] StudentCols() => new[] { Column.T("Name", "Имя"), Column.T("Surname", "Фамилия"), Column.N("Age", "Возраст"), Column.T("Group", "Группа") };
    static List<string[]> StudentSample() => R(A("Иван", "Петров", "19", "ИС-21"), A("Анна", "Сидорова", "18", "ИС-21"), A("Пётр", "Орлов", "14", "ПО-11"));
    static Student MakeStudent(Row r) => new Student(r.Str("Name"), r.Str("Surname"), r.Int("Age"), r.Str("Group"));

    static Column[] TeacherCols() => new[] { Column.T("Name", "Имя"), Column.T("Surname", "Фамилия"), Column.N("Age", "Возраст"), Column.T("Subject", "Предмет"), Column.N("Experience", "Стаж (лет)") };
    static List<string[]> TeacherSample() => R(A("Мария", "Иванова", "42", "Программирование", "15"), A("Олег", "Кузнецов", "55", "Математика", "30"));
    static Teacher MakeTeacher(Row r) => new Teacher(r.Str("Name"), r.Str("Surname"), r.Int("Age"), r.Str("Subject"), r.Int("Experience"));

    static Column[] PersonCols() => new[] { Column.T("Name", "Имя"), Column.T("Surname", "Фамилия"), Column.N("Age", "Возраст") };
    static List<string[]> PersonSample() => R(A("Иван", "Петров", "19"), A("Мария", "Иванова", "42"));

    const string S1 = "1. Создание структуры и применение public/private";
    const string S2 = "2. Управление преподавателями и группами";
    const string S3 = "3. Наследование и protected";
    const string S4 = "4. Модификаторы internal и protected internal";
    const string S5 = "5. Реализация функциональности сайта";

    static IReadOnlyList<TaskDef> Build() => new List<TaskDef>
    {
        // ======================= Раздел 1 =======================
        D(1, S1, "Создание структуры и класса Student",
          "Создайте класс Student с полями Name, Surname, Age, Group. Используйте модификатор public для класса.",
          StudentCols(), StudentSample(), rows =>
          {
              var res = Each(rows, (r, x) => x.Ok("Создан объект Student: " + MakeStudent(r).GetStudentInfo()));
              res.Info("Модификатор доступа класса Student: " + AccessInfo.TypeModifier(typeof(Student)));
              return res;
          }),

        D(2, S1, "Защита персональных данных",
          "Сделайте поля Name, Surname и Age закрытыми с помощью private. Создайте публичные методы для получения информации о студенте.",
          StudentCols(), StudentSample(), rows =>
          {
              var res = Each(rows, (r, x) => x.Ok("Информация через публичный метод: " + MakeStudent(r).GetStudentInfo()));
              var t = AccessInfo.Table(typeof(Person), m => m is FieldInfo);
              t.AddRange(AccessInfo.Table(typeof(Student), m => m is FieldInfo));
              res.Table("Поля и их доступность (в итоговой иерархии Name/Surname/Age вынесены в Person как protected — снаружи они закрыты так же, как private)", AccessInfo.Headers, t);
              return res;
          }),

        D(3, S1, "Создание свойств",
          "Реализуйте свойства Name, Surname, Age, позволяющие безопасно изменять данные студента. Оставьте «Новое …» пустым, если менять не нужно.",
          StudentCols().Concat(new[] { Column.T("NewName", "Новое имя"), Column.T("NewSurname", "Новая фамилия"), Column.T("NewAge", "Новый возраст") }).ToArray(),
          R(A("Иван", "Петров", "19", "ИС-21", "Алексей", "", "20"), A("Анна", "Сидорова", "18", "ИС-21", "", "", "5")),
          rows => Each(rows, (r, x) =>
          {
              var s = MakeStudent(r);
              var before = s.GetStudentInfo();
              if (r.Opt("NewName") != "") s.Name = r.Opt("NewName");
              if (r.Opt("NewSurname") != "") s.Surname = r.Opt("NewSurname");
              if (r.Opt("NewAge") != "") s.Age = r.Int("NewAge");
              x.Ok($"{before}  →  {s.GetStudentInfo()}");
          })),

        D(4, S1, "Проверка возраста",
          "Создайте метод SetAge(), который разрешает устанавливать возраст только от 15 до 70 лет. Используйте private для внутреннего хранения возраста.",
          StudentCols().Concat(new[] { Column.N("NewAge", "Новый возраст") }).ToArray(),
          R(A("Иван", "Петров", "19", "ИС-21", "25"), A("Анна", "Сидорова", "18", "ИС-21", "10"), A("Пётр", "Орлов", "20", "ПО-11", "71")),
          rows => Each(rows, (r, x) =>
          {
              var s = MakeStudent(r);
              var old = s.Age;
              s.SetAge(r.Int("NewAge"));
              x.Ok($"{s.Name} {s.Surname}: возраст {old} → {s.Age}");
          })),

        D(5, S1, "Добавление конструктора",
          "Создайте публичный конструктор класса Student, принимающий имя, фамилию, возраст и группу.",
          StudentCols(), StudentSample(),
          rows => Each(rows, (r, x) => x.Ok("Student(name, surname, age, group) → " + MakeStudent(r).GetStudentInfo()))),

        D(6, S1, "Скрытие внутренних методов",
          "Создайте приватный метод ValidateStudentData(), который проверяет правильность введённых данных. Вызывайте его из конструктора.",
          StudentCols(), StudentSample(), rows =>
          {
              var res = Each(rows, (r, x) =>
              {
                  var s = MakeStudent(r);
                  x.Ok("ValidateStudentData() вызван в конструкторе — данные корректны: " + s.GetStudentInfo());
              });
              res.Table("Методы класса Student", AccessInfo.Headers, AccessInfo.Table(typeof(Student), m => m is MethodInfo));
              return res;
          }),

        D(7, S1, "Отображение информации",
          "Создайте публичный метод GetStudentInfo(), который возвращает полную информацию о студенте.",
          StudentCols(), StudentSample(),
          rows => Each(rows, (r, x) => x.Ok(MakeStudent(r).GetStudentInfo()))),

        D(8, S1, "Запрет прямого изменения данных",
          "Сделайте идентификатор студента StudentId доступным только для чтения после создания объекта.",
          StudentCols().Concat(new[] { Column.N("NewId", "Попытка присвоить ID") }).ToArray(),
          R(A("Иван", "Петров", "19", "ИС-21", "100"), A("Анна", "Сидорова", "18", "ИС-21", "7")),
          rows => Each(rows, (r, x) =>
          {
              var s = MakeStudent(r);
              var prop = typeof(Student).GetProperty(nameof(Student.StudentId));
              var canSet = prop.SetMethod != null && prop.SetMethod.IsPublic;
              x.Ok($"StudentId = {s.StudentId}. Попытка присвоить {r.Opt("NewId")}: "
                   + (canSet ? "разрешено" : "запрещено — у свойства нет публичного set (в коде это ошибка компиляции CS0200)"));
          })),

        // ======================= Раздел 2 =======================
        D(9, S2, "Создание класса Teacher",
          "Добавьте класс преподавателя с полями имени, фамилии, предмета и стажа. Используйте private для полей и public для необходимых свойств.",
          TeacherCols(), TeacherSample(), rows =>
          {
              var res = Each(rows, (r, x) => x.Ok(MakeTeacher(r).GetTeacherInfo()));
              res.Table("Поля и свойства Teacher", AccessInfo.Headers, AccessInfo.Table(typeof(Teacher), m => m is FieldInfo || m is PropertyInfo));
              return res;
          }),

        D(10, S2, "Ограничение изменения стажа",
          "Создайте свойство Experience, которое позволяет изменять стаж только через специальный метод UpdateExperience().",
          TeacherCols().Concat(new[] { Column.N("NewExperience", "Новый стаж") }).ToArray(),
          R(A("Мария", "Иванова", "42", "Программирование", "15", "16"), A("Олег", "Кузнецов", "55", "Математика", "30", "10")),
          rows => Each(rows, (r, x) =>
          {
              var t = MakeTeacher(r);
              var old = t.Experience;
              t.UpdateExperience(r.Int("NewExperience"));
              x.Ok($"{t.Surname} {t.Name}: стаж {old} → {t.Experience}");
          })),

        D(11, S2, "Создание класса Group",
          "Добавьте класс учебной группы с названием, курсом и списком студентов. Список студентов сделайте закрытым (private).",
          new[] { Column.T("Title", "Название"), Column.N("Course", "Курс") },
          R(A("ИС-21", "2"), A("ПО-11", "1"), A("ТХ-31", "9")),
          rows =>
          {
              var res = Each(rows, (r, x) =>
              {
                  var g = new Group(r.Str("Title"), r.Int("Course"));
                  x.Ok($"Группа {g.Title}, курс {g.Course}, студентов: {g.Students.Count}");
              });
              res.Table("Члены класса Group", AccessInfo.Headers, AccessInfo.Table(typeof(Group), m => m is FieldInfo || m is PropertyInfo));
              return res;
          }),

        D(12, S2, "Добавление студентов в группу",
          "Создайте публичный метод AddStudent(). Он должен добавлять студента только при условии, что он ещё не состоит в группе.",
          new[] { Column.T("GroupTitle", "Группа"), Column.T("Name", "Имя"), Column.T("Surname", "Фамилия"), Column.N("Age", "Возраст") },
          R(A("ИС-21", "Иван", "Петров", "19"), A("ИС-21", "Анна", "Сидорова", "18"), A("ИС-21", "Иван", "Петров", "19"), A("ПО-11", "Пётр", "Орлов", "17")),
          rows =>
          {
              var groups = new Dictionary<string, Group>(StringComparer.OrdinalIgnoreCase);
              var res = Each(rows, (r, x) =>
              {
                  var title = r.Str("GroupTitle");
                  if (!groups.TryGetValue(title, out var g)) { g = new Group(title, 1); groups[title] = g; }
                  var s = new Student(r.Str("Name"), r.Str("Surname"), r.Int("Age"), title);
                  if (g.AddStudent(s)) x.Ok($"{s.Surname} {s.Name} добавлен в группу {title}");
                  else x.Err($"{s.Surname} {s.Name} уже состоит в группе {title} — не добавлен");
              });
              foreach (var g in groups.Values)
                  res.Table($"Состав группы {g.Title}", new[] { "ID", "Студент", "Возраст" },
                      g.Students.Select(s => new[] { s.StudentId.ToString(), $"{s.Surname} {s.Name}", s.Age.ToString() }).ToList());
              return res;
          }),

        D(13, S2, "Удаление студентов",
          "Создайте метод RemoveStudent(). Запретите прямое удаление элементов из списка студентов извне класса.",
          new[] { Column.S("Action", "Действие", "Добавить", "Удалить"), Column.T("Name", "Имя"), Column.T("Surname", "Фамилия"), Column.N("Age", "Возраст (для добавления)") },
          R(A("Добавить", "Иван", "Петров", "19"), A("Добавить", "Анна", "Сидорова", "18"), A("Удалить", "Иван", "Петров", ""), A("Удалить", "Сергей", "Орлов", "")),
          rows =>
          {
              var g = new Group("ИС-21", 2);
              var res = Each(rows, (r, x) =>
              {
                  if (r.Str("Action") == "Добавить")
                  {
                      var s = new Student(r.Str("Name"), r.Str("Surname"), r.Int("Age"), g.Title);
                      x.Ok(g.AddStudent(s) ? $"Добавлен: {s.Surname} {s.Name}" : $"{s.Surname} {s.Name} уже в группе");
                  }
                  else
                  {
                      var removed = g.RemoveStudent(r.Str("Name"), r.Str("Surname"));
                      if (removed) x.Ok($"Удалён: {r.Str("Surname")} {r.Str("Name")}");
                      else x.Err($"Студент {r.Str("Surname")} {r.Str("Name")} в группе не найден");
                  }
              });
              res.Info("Students возвращает IReadOnlyList — вызов g.Students.Add()/Remove() снаружи не скомпилируется, изменять состав можно только методами группы.");
              res.Table("Итоговый состав группы ИС-21", new[] { "Студент", "Возраст" },
                  g.Students.Select(s => new[] { $"{s.Surname} {s.Name}", s.Age.ToString() }).ToList());
              return res;
          }),

        D(14, S2, "Создание класса Subject",
          "Добавьте класс учебного предмета. Используйте закрытые поля для названия, количества часов и преподавателя.",
          new[] { Column.T("Title", "Название"), Column.N("Hours", "Часов") },
          R(A("Программирование", "120"), A("Математика", "90"), A("История", "0")),
          rows =>
          {
              var res = Each(rows, (r, x) => x.Ok(new Subject(r.Str("Title"), r.Int("Hours")).GetInfo()));
              res.Table("Поля Subject", AccessInfo.Headers, AccessInfo.Table(typeof(Subject), m => m is FieldInfo));
              return res;
          }),

        D(15, S2, "Связь преподавателя с предметом",
          "Реализуйте метод AssignTeacher(), который назначает преподавателя на предмет. Сделайте проверку, что преподаватель не равен null.",
          new[] { Column.T("Title", "Предмет"), Column.N("Hours", "Часов"), Column.T("TName", "Имя преподавателя"), Column.T("TSurname", "Фамилия преподавателя"), Column.S("Assign", "Назначить?", "Да", "Нет") },
          R(A("Программирование", "120", "Мария", "Иванова", "Да"), A("Математика", "90", "Олег", "Кузнецов", "Нет")),
          rows => Each(rows, (r, x) =>
          {
              var subj = new Subject(r.Str("Title"), r.Int("Hours"));
              Teacher t = r.Str("Assign") == "Да"
                  ? new Teacher(r.Str("TName"), r.Str("TSurname"), 40, r.Str("Title"), 5)
                  : null;
              subj.AssignTeacher(t);
              x.Ok(subj.GetInfo());
          })),

        // ======================= Раздел 3 =======================
        D(16, S3, "Создание базового класса Person",
          "Создайте класс Person с именем, фамилией и возрастом. Используйте protected для полей, которые должны быть доступны наследникам.",
          PersonCols(), PersonSample(), rows =>
          {
              var res = Each(rows, (r, x) => x.Ok("Person: " + new Person(r.Str("Name"), r.Str("Surname"), r.Int("Age")).Describe()));
              res.Table("Поля Person", AccessInfo.Headers, AccessInfo.Table(typeof(Person), m => m is FieldInfo));
              return res;
          }),

        D(17, S3, "Наследование класса Student",
          "Измените класс Student, чтобы он наследовался от Person. Уберите повторяющиеся поля.",
          StudentCols(), StudentSample(), rows =>
          {
              var res = Each(rows, (r, x) => x.Ok(MakeStudent(r).GetStudentInfo()));
              res.Info("Базовый класс Student: " + typeof(Student).BaseType.Name + ". Поля Name/Surname/Age в Student не дублируются.");
              res.Table("Собственные поля и свойства Student", AccessInfo.Headers, AccessInfo.Table(typeof(Student), m => m is FieldInfo || m is PropertyInfo));
              return res;
          }),

        D(18, S3, "Наследование класса Teacher",
          "Сделайте класс Teacher наследником Person. Реализуйте отдельный метод для отображения информации о преподавателе.",
          TeacherCols(), TeacherSample(), rows =>
          {
              var res = Each(rows, (r, x) => x.Ok(MakeTeacher(r).GetTeacherInfo()));
              res.Info("Базовый класс Teacher: " + typeof(Teacher).BaseType.Name);
              return res;
          }),

        D(19, S3, "Использование protected-методов",
          "Добавьте в Person защищённый метод DisplayBasicInfo(). Используйте его внутри классов Student и Teacher.",
          new[] { Column.S("Kind", "Кто", "Студент", "Преподаватель"), Column.T("Name", "Имя"), Column.T("Surname", "Фамилия"), Column.N("Age", "Возраст"), Column.T("Extra", "Группа / Предмет") },
          R(A("Студент", "Иван", "Петров", "19", "ИС-21"), A("Преподаватель", "Мария", "Иванова", "42", "Программирование")),
          rows => Each(rows, (r, x) =>
          {
              var text = r.Str("Kind") == "Студент"
                  ? new Student(r.Str("Name"), r.Str("Surname"), r.Int("Age"), r.Str("Extra")).GetStudentInfo()
                  : new Teacher(r.Str("Name"), r.Str("Surname"), r.Int("Age"), r.Str("Extra"), 0).GetTeacherInfo();
              x.Ok("DisplayBasicInfo() использован внутри наследника: " + text);
          })),

        D(20, S3, "Ограничение доступа к данным",
          "Проверьте, какие поля и методы базового класса доступны внутри наследников, а какие доступны только внутри самого класса. Введите имена членов Person.",
          new[] { Column.T("Member", "Член класса Person") },
          R(A("_name"), A("_age"), A("_passportNote"), A("DisplayBasicInfo"), A("GetFullName"), A("CalculateStatistics"), A("InternalTag"), A("Describe"), A("Unknown")),
          rows =>
          {
              var res = new TaskResult();
              var list = new List<string[]>();
              const BindingFlags f = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
              foreach (var r in rows)
              {
                  var name = r.Opt("Member");
                  var m = typeof(Person).GetMember(name, f).FirstOrDefault(z => z is FieldInfo || z is MethodInfo || z is PropertyInfo);
                  if (m == null) { list.Add(new[] { name, "—", "не найден", "—", "—" }); continue; }
                  var mod = AccessInfo.Modifier(m);
                  list.Add(new[] { name, AccessInfo.Kind(m), mod, AccessInfo.InDerived(mod) ? "Да" : "Нет", AccessInfo.Outside(mod) ? "Да" : "Нет" });
              }
              res.Table("Доступность членов Person", AccessInfo.Headers, list);
              res.Info("«Снаружи» — код вне наследников и вне сборки. Внутри наследников доступно всё, кроме private.");
              return res;
          }),

        // ======================= Раздел 4 =======================
        D(21, S4, "Создание класса CollegeSettings",
          "Создайте класс настроек колледжа с модификатором internal. Он должен хранить название колледжа и учебный год.",
          new[] { Column.T("CollegeName", "Название колледжа"), Column.T("AcademicYear", "Учебный год") },
          R(A("Технический колледж", "2026/2027")),
          rows =>
          {
              var res = Each(rows, (r, x) => x.Ok(new CollegeSettings(r.Str("CollegeName"), r.Str("AcademicYear")).Describe()));
              res.Info("Модификатор класса CollegeSettings: " + AccessInfo.TypeModifier(typeof(CollegeSettings)));
              return res;
          }),

        D(22, S4, "Ограничение доступа к настройкам",
          "Сделайте методы изменения настроек доступными только внутри текущей сборки проекта с помощью internal.",
          new[] { Column.T("CollegeName", "Название колледжа"), Column.T("AcademicYear", "Учебный год"), Column.T("NewName", "Новое название"), Column.T("NewYear", "Новый год") },
          R(A("Технический колледж", "2026/2027", "Политехнический колледж", "2027/2028")),
          rows =>
          {
              var res = Each(rows, (r, x) =>
              {
                  var cs = new CollegeSettings(r.Str("CollegeName"), r.Str("AcademicYear"));
                  var before = cs.Describe();
                  cs.Update(r.Str("NewName"), r.Str("NewYear"));
                  x.Ok($"{before}  →  {cs.Describe()}");
              });
              var upd = typeof(CollegeSettings).GetMethod("Update", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
              res.Info("Модификатор метода Update(): " + AccessInfo.Modifier(upd) + " — из другой сборки он недоступен.");
              return res;
          }),

        D(23, S4, "Создание protected internal метода",
          "Добавьте в базовый класс Person метод protected internal GetFullName(). Проверьте его доступность внутри сборки и в наследуемых классах.",
          PersonCols(), PersonSample(),
          rows => Each(rows, (r, x) =>
          {
              var p = new Person(r.Str("Name"), r.Str("Surname"), r.Int("Age"));
              x.Ok("Вызов внутри сборки: Person.GetFullName() = " + p.GetFullName());
              var s = new Student(r.Str("Name"), r.Str("Surname"), r.Int("Age"), "Без группы");
              x.Ok("Вызов из наследника Student: " + s.FullNameViaProtectedInternal());
          })),

        D(24, S4, "Разделение доступа к отчётам",
          "Создайте класс ReportManager с методом protected internal GenerateReport(). Используйте его в соответствующих классах проекта.",
          new[] { Column.T("Student", "Студент"), Column.T("Subject", "Предмет"), Column.N("Grade", "Оценка") },
          R(A("Иван Петров", "Программирование", "5"), A("Анна Сидорова", "Математика", "4"), A("Пётр Орлов", "История", "7")),
          rows =>
          {
              var lines = new List<string>();
              var res = Each(rows, (r, x) =>
              {
                  var g = r.Int("Grade");
                  if (g < 2 || g > 5) throw new ArgumentException("Оценка должна быть от 2 до 5");
                  lines.Add($"{r.Str("Student")} — {r.Str("Subject")}: {g}");
              });
              res.Info(new GradeReport().Build("Отчёт по оценкам", lines));
              var gm = typeof(ReportManager).GetMethod("GenerateReport", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
              res.Info("Модификатор ReportManager.GenerateReport(): " + AccessInfo.Modifier(gm) + " — доступен наследнику GradeReport и всей сборке.");
              return res;
          }),

        D(25, S4, "Применение private protected",
          "Создайте метод private protected CalculateStatistics() в базовом классе. Проверьте, что он доступен наследникам только внутри той же сборки.",
          PersonCols(), PersonSample(), rows =>
          {
              var res = Each(rows, (r, x) =>
              {
                  var s = new Student(r.Str("Name"), r.Str("Surname"), r.Int("Age"), "Без группы");
                  x.Ok($"{s.Surname} {s.Name}: CalculateStatistics() через наследника = {s.StatsViaPrivateProtected()} (возраст в днях)");
              });
              var m = typeof(Person).GetMethod("CalculateStatistics", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
              res.Info("Модификатор CalculateStatistics(): " + AccessInfo.Modifier(m) + " — прямой вызов person.CalculateStatistics() снаружи не скомпилируется.");
              return res;
          }),

        // ======================= Раздел 5 =======================
        D(26, S5, "Система авторизации",
          "Создайте класс UserAccount с приватными полями логина и пароля. Реализуйте публичные методы Login() и Logout(). Пароль не должен отображаться при выводе информации.",
          new[] { Column.T("Login", "Логин"), Column.T("Password", "Пароль"), Column.T("TryLogin", "Попытка: логин"), Column.T("TryPassword", "Попытка: пароль") },
          R(A("admin", "secret1", "admin", "secret1"), A("user", "qwerty", "user", "wrong")),
          rows => Each(rows, (r, x) =>
          {
              var acc = new UserAccount(r.Str("Login"), r.Str("Password"));
              x.Info(acc.ToString());
              if (acc.Login(r.Opt("TryLogin"), r.Opt("TryPassword"))) x.Ok($"Вход выполнен ({r.Opt("TryLogin")}). IsLoggedIn = {acc.IsLoggedIn}");
              else x.Err($"Неверный логин или пароль ({r.Opt("TryLogin")}). IsLoggedIn = {acc.IsLoggedIn}");
              acc.Logout();
              x.Info("После Logout(): IsLoggedIn = " + acc.IsLoggedIn);
          })),

        D(27, S5, "Разделение ролей пользователей",
          "Создайте базовый класс User и наследников Admin, Teacher и Student. Используйте protected для общих данных и private для конфиденциальных данных.",
          new[] { Column.T("FullName", "ФИО"), Column.T("Password", "Пароль"), Column.S("Role", "Роль", "Admin", "Teacher", "Student") },
          R(A("Администратор Системы", "admin123", "Admin"), A("Мария Иванова", "teach456", "Teacher"), A("Иван Петров", "stud789", "Student")),
          rows => Each(rows, (r, x) =>
          {
              User u = r.Str("Role") switch
              {
                  "Admin" => new AdminUser(r.Str("FullName"), r.Str("Password")),
                  "Teacher" => new TeacherUser(r.Str("FullName"), r.Str("Password")),
                  "Student" => new StudentUser(r.Str("FullName"), r.Str("Password")),
                  _ => throw new ArgumentException("Неизвестная роль")
              };
              x.Ok($"{u.Describe()}. Пароль: {u.PasswordMask()}");
          })),

        D(28, S5, "Журнал оценок",
          "Создайте класс GradeBook. Список оценок должен быть закрытым. Добавление и удаление оценок выполняйте через публичные методы с проверкой допустимых значений от 2 до 5.",
          new[] { Column.S("Operation", "Операция", "Добавить", "Удалить"), Column.N("Grade", "Оценка") },
          R(A("Добавить", "5"), A("Добавить", "4"), A("Добавить", "3"), A("Добавить", "7"), A("Удалить", "3"), A("Удалить", "2")),
          rows =>
          {
              var gb = new GradeBook();
              var res = Each(rows, (r, x) =>
              {
                  var g = r.Int("Grade");
                  if (r.Str("Operation") == "Добавить") { gb.AddGrade(g); x.Ok($"Оценка {g} добавлена"); }
                  else if (gb.RemoveGrade(g)) x.Ok($"Оценка {g} удалена");
                  else x.Err($"Оценки {g} в журнале нет");
              });
              res.Info($"Оценки в журнале: {(gb.Grades.Count == 0 ? "—" : string.Join(", ", gb.Grades))}; средний балл: {gb.Average:F2}");
              return res;
          }),

        D(29, S5, "Система посещаемости",
          "Создайте класс AttendanceManager. Внутренние данные о посещаемости сделайте приватными. Реализуйте методы добавления, изменения и получения статистики посещаемости.",
          new[] { Column.S("Operation", "Операция", "Добавить", "Изменить"), Column.T("Student", "Студент"), Column.D("Date", "Дата"), Column.S("Status", "Статус", "Присутствовал", "Отсутствовал") },
          R(A("Добавить", "Иван Петров", "2026-10-26", "Присутствовал"), A("Добавить", "Иван Петров", "2026-10-27", "Отсутствовал"),
            A("Добавить", "Анна Сидорова", "2026-10-26", "Присутствовал"), A("Изменить", "Иван Петров", "2026-10-27", "Присутствовал"),
            A("Изменить", "Анна Сидорова", "2026-10-30", "Отсутствовал")),
          rows =>
          {
              var am = new AttendanceManager();
              var res = Each(rows, (r, x) =>
              {
                  var present = r.Str("Status") == "Присутствовал";
                  if (r.Str("Operation") == "Добавить") am.Add(r.Str("Student"), r.Str("Date"), present);
                  else am.Change(r.Str("Student"), r.Str("Date"), present);
                  x.Ok($"{r.Str("Operation")}: {r.Str("Student")}, {r.Str("Date")} — {r.Str("Status")}");
              });
              res.Table("Статистика посещаемости", new[] { "Студент", "Занятий", "Был", "Посещаемость, %" },
                  am.GetStatistics().Select(s => new[] { s.Student, s.Total.ToString(), s.Present.ToString(), s.Percent.ToString() }).ToList());
              return res;
          }),

        D(30, S5, "Финальная сборка проекта",
          "Объедините все созданные классы в полноценное веб-приложение: страница авторизации; главная страница колледжа; список студентов; страница преподавателей; страницы учебных групп; электронный журнал оценок; страница посещаемости; административная панель.",
          Array.Empty<Column>(), new List<string[]>(), null),
    };
}
