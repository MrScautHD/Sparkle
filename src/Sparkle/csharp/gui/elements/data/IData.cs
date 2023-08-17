using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparkle.csharp.gui.elements.data;
public interface IData<T>
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns>A shallow copy of this data set</returns>
    T Clone();
}
