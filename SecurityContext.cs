using System;
using System.ServiceModel;

namespace Bisoyi.DB
{
    public class SecurityContext
    {
        public string User { get; set; }

        public static SecurityContext Current
        {
            get
            {
                try
                {
                    //retrieve the value from any context suitable in your application
                    //HTTPContext, WCF Service operation context etc..
                    //In this example, prior to execution of this code, SecurityContext has been already set
                    return OperationContext.Current.Extensions.Find<SecurityContext>();
                }
                catch
                {
                    throw new Exception("Security context not found.");
                }
            }
        }
    }
}