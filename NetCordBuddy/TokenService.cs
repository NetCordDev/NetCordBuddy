using NetCord;

namespace NetCordBuddy;

internal class TokenService
{
    public Token Token { get; }

    public TokenService(ConfigService config)
    {
        Token = new(TokenType.Bot, config.Token);
    }
}