using CDTk;

namespace Badger;

public class WASMgrammer
{
    class Tokens : TokenSet
    {
        public Token Whitespace = new Token(@"\s+").Ignore();
    }
}