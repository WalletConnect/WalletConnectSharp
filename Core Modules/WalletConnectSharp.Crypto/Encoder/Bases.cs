namespace WalletConnectSharp.Crypto.Encoder
{
    public static class Bases
    {
        public static Codec Base10 = new Codec(
            "base10",
            "9",
            new BaseX(
                "0123456789",
                "base10"
            )
        );
    }
}