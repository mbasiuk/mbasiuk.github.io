class LoginOptions
{
    public const string DefaultSection = "LoginOptions";
    public string User { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Salt { get; set; } = null!;

    public string Hash(LoginRecord login)
    {
        return (login.user + login.password + Salt).GetHashCode().ToString();
    }

    public string Hash()
    {
        return (User + Password + Salt).GetHashCode().ToString();
    }

    public bool IsValid(LoginRecord login)
    {
        return (User != null) && (Password != null) && (login != null) && (login.user == User) && (login.password == Password);
    }
}
