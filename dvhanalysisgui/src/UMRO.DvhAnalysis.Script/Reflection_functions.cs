using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UMRO.DvhAnalysis.Script
{
    public class Reflection_functions
    {

        public static T GetPropertyValue<T>(object obj, string propName) 
        { 
            return (T)obj.GetType().GetProperty(propName).GetValue(obj, null); 
        }


    }
}
