// Can use classes from the listed namespaces.
using System;

// The container for classes and other namespaces.
namespace Landis.Extension.SOSIELHuman.Helpers
{
    public static class VariableTypeHelper
    {
        /// <summary>
        /// Converts type name to System.Type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Unknown variable type:" + type</exception>
        public static Type ConvertStringToType(string type)
        {
            switch (type)
            {
                case "integer": return typeof(int);
                case "number": return typeof(double);
                case "string": return typeof(string);
                default:
                    throw new ArgumentOutOfRangeException("Unknown variable type:" + type);
            }
        }

    }
}
/*
Copyright 2020 Garry Sotnik

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
