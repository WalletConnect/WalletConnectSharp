namespace WalletConnectSharp.Events
{
    [System.AttributeUsage(System.AttributeTargets.Class |  
                           System.AttributeTargets.Struct)  
    ]  
    public class RegisterMethodAttribute : System.Attribute
    {
        public string methodName;

        public RegisterMethodAttribute(string methodName)
        {
            this.methodName = methodName;
        }
    }
}