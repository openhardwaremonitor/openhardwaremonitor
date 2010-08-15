/*
  
  Version: MPL 1.1/GPL 2.0/LGPL 2.1

  The contents of this file are subject to the Mozilla Public License Version
  1.1 (the "License"); you may not use this file except in compliance with
  the License. You may obtain a copy of the License at
 
  http://www.mozilla.org/MPL/

  Software distributed under the License is distributed on an "AS IS" basis,
  WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
  for the specific language governing rights and limitations under the License.

  The Original Code is the Open Hardware Monitor code.

  The Initial Developer of the Original Code is 
  Michael Möller <m.moeller@gmx.ch>.
  Portions created by the Initial Developer are Copyright (C) 2009-2010
  the Initial Developer. All Rights Reserved.

  Contributor(s):

  Alternatively, the contents of this file may be used under the terms of
  either the GNU General Public License Version 2 or later (the "GPL"), or
  the GNU Lesser General Public License Version 2.1 or later (the "LGPL"),
  in which case the provisions of the GPL or the LGPL are applicable instead
  of those above. If you wish to allow use of your version of this file only
  under the terms of either the GPL or the LGPL, and not to allow others to
  use your version of this file under the terms of the MPL, indicate your
  decision by deleting the provisions above and replace them with the notice
  and other provisions required by the GPL or the LGPL. If you do not delete
  the provisions above, a recipient may use your version of this file under
  the terms of any one of the MPL, the GPL or the LGPL.
 
*/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace OpenHardwareMonitor.Hardware {

  internal sealed class PInvokeDelegateFactory {

    private static ModuleBuilder moduleBuilder = 
      AppDomain.CurrentDomain.DefineDynamicAssembly(
        new AssemblyName("PInvokeDelegateFactoryInternalAssembly"),
        AssemblyBuilderAccess.Run).DefineDynamicModule(
        "PInvokeDelegateFactoryInternalModule");

    private static IDictionary<DllImportAttribute, Type> wrapperTypes =
      new Dictionary<DllImportAttribute, Type>();

    private PInvokeDelegateFactory() { }

    public static void CreateDelegate<T>(DllImportAttribute dllImportAttribute,
      out T newDelegate) where T : class 
    {
      Type wrapperType;
      wrapperTypes.TryGetValue(dllImportAttribute, out wrapperType);

      if (wrapperType == null) {
        wrapperType = CreateWrapperType(typeof(T), dllImportAttribute);
        wrapperTypes.Add(dllImportAttribute, wrapperType);
      }

      newDelegate = Delegate.CreateDelegate(typeof(T), wrapperType,
        dllImportAttribute.EntryPoint) as T;
    }


    private static Type CreateWrapperType(Type delegateType,
      DllImportAttribute dllImportAttribute) {

      TypeBuilder typeBuilder = moduleBuilder.DefineType(
        "PInvokeDelegateFactoryInternalWrapperType" + wrapperTypes.Count);

      MethodInfo methodInfo = delegateType.GetMethod("Invoke");

      ParameterInfo[] parameterInfos = methodInfo.GetParameters();
      int parameterCount = parameterInfos.GetLength(0);

      Type[] parameterTypes = new Type[parameterCount];
      for (int i = 0; i < parameterCount; i++)
        parameterTypes[i] = parameterInfos[i].ParameterType;

      MethodBuilder methodBuilder = typeBuilder.DefinePInvokeMethod(
        dllImportAttribute.EntryPoint, dllImportAttribute.Value,
        MethodAttributes.Public | MethodAttributes.Static |
        MethodAttributes.PinvokeImpl, CallingConventions.Standard,
        methodInfo.ReturnType, parameterTypes,
        dllImportAttribute.CallingConvention,
        dllImportAttribute.CharSet);

      foreach (ParameterInfo parameterInfo in parameterInfos)
        methodBuilder.DefineParameter(parameterInfo.Position + 1,
          parameterInfo.Attributes, parameterInfo.Name);

      if (dllImportAttribute.PreserveSig)
        methodBuilder.SetImplementationFlags(MethodImplAttributes.PreserveSig);

      return typeBuilder.CreateType();
    }
  }
}
