namespace dailycue_api;

public class Env
{
    public required ConnectionStrings ConnectionStrings { get; set; }
    public required Jwt Jwt { get; set; }

    public void Validate()
    {
        var missing = new List<string>();

        if (string.IsNullOrWhiteSpace(ConnectionStrings?.DefaultConnection))
            missing.Add("ConnectionStrings:DefaultConnection");

        if (string.IsNullOrWhiteSpace(Jwt?.Key))
            missing.Add("Jwt:Key");

        if (string.IsNullOrWhiteSpace(Jwt?.Issuer))
            missing.Add("Jwt:Issuer");

        if (string.IsNullOrWhiteSpace(Jwt?.Audience))
            missing.Add("Jwt:Audience");

        if (missing.Count > 0)
            throw new Exception("Variáveis/Configs faltando: " + string.Join(", ", missing));
    }
}

public class ConnectionStrings
{
    public required string DefaultConnection { get; set; }
}

public class Jwt
{
    public required string Key { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
}