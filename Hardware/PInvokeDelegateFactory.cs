/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2012 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using OpenHardwareMonitor.Collections;

namespace OpenHardwareMonitor.Hardware
{
    internal static class PInvokeDelegateFactory
    {
        private static readonly ModuleBuilder moduleBuilder =
            AppDomain.CurrentDomain.DefineDynamicAssembly(
                new AssemblyName("PInvokeDelegateFactoryInternalAssembly"),
                AssemblyBuilderAccess.Run).DefineDynamicModule(
                "PInvokeDelegateFactoryInternalModule");

        private static readonly IDictionary<Pair<DllImportAttribute, Type>, Type> wrapperTypes =
            new Dictionary<Pair<DllImportAttribute, Type>, Type>();

        public static void CreateDelegate<T>(DllImportAttribute dllImportAttribute,
            out T newDelegate) where T : class
        {
            var key =
                new Pair<DllImportAttribute, Type>(dllImportAttribute, typeof(T));
            wrapperTypes.TryGetValue(key, out Type wrapperType);

            if (wrapperType == null)
            {
                wrapperType = CreateWrapperType(typeof(T), dllImportAttribute);
                wrapperTypes.Add(key, wrapperType);
            }

            newDelegate = Delegate.CreateDelegate(typeof(T), wrapperType,
                dllImportAttribute.EntryPoint) as T;
        }


        private static Type CreateWrapperType(Type delegateType,
            DllImportAttribute dllImportAttribute)
        {
            var typeBuilder = moduleBuilder.DefineType(
                "PInvokeDelegateFactoryInternalWrapperType" + wrapperTypes.Count);

            var methodInfo = delegateType.GetMethod("Invoke");

            var parameterInfos = methodInfo.GetParameters();
            var parameterCount = parameterInfos.GetLength(0);

            var parameterTypes = new Type[parameterCount];
            for (var i = 0; i < parameterCount; i++)
                parameterTypes[i] = parameterInfos[i].ParameterType;

            var methodBuilder = typeBuilder.DefinePInvokeMethod(
                dllImportAttribute.EntryPoint, dllImportAttribute.Value,
                MethodAttributes.Public | MethodAttributes.Static |
                MethodAttributes.PinvokeImpl, CallingConventions.Standard,
                methodInfo.ReturnType, parameterTypes,
                dllImportAttribute.CallingConvention,
                dllImportAttribute.CharSet);

            foreach (var parameterInfo in parameterInfos)
                methodBuilder.DefineParameter(parameterInfo.Position + 1,
                    parameterInfo.Attributes, parameterInfo.Name);

            if (dllImportAttribute.PreserveSig)
                methodBuilder.SetImplementationFlags(MethodImplAttributes.PreserveSig);

            return typeBuilder.CreateType();
        }
    }
}