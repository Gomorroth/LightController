using gomoru.su.LightController.API;
using gomoru.su.LightController.API.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace gomoru.su.LightController
{
    partial class RuntimeHelper
    {
        private static class ShaderSettings
        {
            private static readonly FieldInfo NameField = typeof(Parameter).GetField(nameof(Parameter.Name));
            public static void Initialize()
            {

                API.ShaderSettings._GetParameters = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(x => x.GetTypes().Where(y => typeof(API.ShaderSettings).IsAssignableFrom(y)))
                    .Select(type =>
                    {
                        var list = new List<(FieldInfo Parent, FieldInfo Field)>();
                        
                        foreach(var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                        {
                            if (typeof(Parameter).IsAssignableFrom(field.FieldType))
                            {
                                list.Add((null, field));
                            }
                            if (typeof(ParameterGroup).IsAssignableFrom(field.FieldType))
                            {
                                foreach (var field2 in field.FieldType.GetFields(BindingFlags.Public | BindingFlags.Instance))
                                {
                                    if (typeof(Parameter).IsAssignableFrom(field2.FieldType))
                                    {
                                        list.Add((field, field2));
                                    }
                                }
                            }
                        }

                        var parameters = list.AsSpan();
                        var method = new DynamicMethod("", typeof(Parameter[]), new[] { typeof(API.ShaderSettings) });
                        var il = method.GetILGenerator();

                        il.DeclareLocal(typeof(ParameterGroup));

                        il.LdcI4(list.Count);
                        il.Emit(OpCodes.Newarr, typeof(Parameter));
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            var parameter = parameters[i];

                            il.Emit(OpCodes.Dup);

                            il.LdcI4(i);
                            il.Emit(OpCodes.Ldarg_0);
                            if (parameter.Parent != null)
                            {
                                il.Emit(OpCodes.Ldfld, parameter.Parent);
                                il.Emit(OpCodes.Dup);
                                il.Emit(OpCodes.Stloc_0);
                            }
                            il.Emit(OpCodes.Ldfld, parameter.Field);
                            il.Emit(OpCodes.Dup);
                            il.Emit(OpCodes.Ldfld, NameField);

                            var label = il.DefineLabel();
                            il.Emit(OpCodes.Ldnull);
                            il.Emit(OpCodes.Ceq);
                            il.Emit(OpCodes.Brtrue_S, label);
                            il.Emit(OpCodes.Dup);

                            var prefix = typeof(ParameterGroup).GetProperty(nameof(ParameterGroup.UseGroupNameAsPrefix));
                            var name = typeof(ParameterGroup).GetProperty(nameof(ParameterGroup.Name));

                            if (parameter.Field.GetCustomAttribute<ParameterNameAttribute>() is ParameterNameAttribute attr)
                            {
                                il.Emit(OpCodes.Ldstr, attr.Name);
                            }
                            else
                            {
                                il.Emit(OpCodes.Ldstr, "_");
                                il.Emit(OpCodes.Ldnull);
                                il.Emit(OpCodes.Ldstr, parameter.Field.Name);
                                il.Emit(OpCodes.Call, typeof(string).GetMethod(nameof(string.Concat), new[] { typeof(string), typeof(string), typeof(string) }));
                            }

                            il.Emit(OpCodes.Stfld, NameField);

                            il.MarkLabel(label);
                            il.Emit(OpCodes.Stelem_Ref);
                        }
                        il.Emit(OpCodes.Ret);

                        return (Type: type, Method: method.CreateDelegate(typeof(Func<API.ShaderSettings, Parameter[]>)) as Func<API.ShaderSettings, Parameter[]>);
                    }).ToImmutableDictionary(x => x.Type, x => x.Method);
            }
        }
    }
}
