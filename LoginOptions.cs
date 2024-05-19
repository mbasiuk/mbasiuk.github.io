class SignInOptions
{
    public const string DefaultSection = "LoginOptions";
    public string User { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Secret { get; set; } = null!;

    public bool IsValid(SignInRecord singIn)
    {
        return (User != null)
        && (Password != null)
        && (singIn.User == User)
        && (singIn.Password == Password);
    }
}
