namespace AA_App.Models;

/// <summary>Задание 26: учётная запись, пароль скрыт.</summary>
public class UserAccount
{
    private string _login;
    private string _password;

    public bool IsLoggedIn { get; private set; }

    public UserAccount(string login, string password)
    {
        if (string.IsNullOrWhiteSpace(login)) throw new ArgumentException("Логин обязателен");
        if (string.IsNullOrWhiteSpace(password) || password.Length < 4) throw new ArgumentException("Пароль должен содержать минимум 4 символа");
        _login = login.Trim();
        _password = password;
    }

    public bool Login(string login, string password)
    {
        IsLoggedIn = login == _login && password == _password;
        return IsLoggedIn;
    }

    public void Logout() => IsLoggedIn = false;

    public override string ToString() => $"Логин: {_login}, пароль: {new string('*', _password.Length)}";
}

/// <summary>Задание 27: роли пользователей.</summary>
public abstract class User
{
    protected string _fullName;
    protected string _role;
    private string _password;                       // конфиденциально

    protected User(string fullName, string password, string role)
    {
        if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("ФИО обязательно");
        if (string.IsNullOrWhiteSpace(password) || password.Length < 4) throw new ArgumentException("Пароль должен содержать минимум 4 символа");
        _fullName = fullName.Trim();
        _password = password;
        _role = role;
    }

    public bool CheckPassword(string password) => _password == password;
    public string PasswordMask() => new string('*', _password.Length);
    public virtual string Describe() => $"{_role}: {_fullName}";
}

public class AdminUser : User
{
    public AdminUser(string fullName, string password) : base(fullName, password, "Администратор") { }
    public override string Describe() => base.Describe() + " — права: полный доступ, управление пользователями";
}

public class TeacherUser : User
{
    public TeacherUser(string fullName, string password) : base(fullName, password, "Преподаватель") { }
    public override string Describe() => base.Describe() + " — права: журнал оценок и посещаемость";
}

public class StudentUser : User
{
    public StudentUser(string fullName, string password) : base(fullName, password, "Студент") { }
    public override string Describe() => base.Describe() + " — права: только просмотр";
}
