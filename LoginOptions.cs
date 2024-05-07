class LoginOptions
{
    public const string DefaultSection = "LoginOptions";
    public string User { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Secret { get; set; } = null!;

    public bool IsValid(LoginRecord login)
    {
        return (User != null)
        && (Password != null)
        && (login != null)
        && (login.User == User)
        && (login.Password == Password);
    }
}
