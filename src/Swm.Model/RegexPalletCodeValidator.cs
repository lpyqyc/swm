// Copyright 2020 王建军
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Text.RegularExpressions;

namespace Swm.Model
{
    public class RegexPalletCodeValidator : IPalletCodeValidator
    {
        readonly string _pattern;

        public RegexPalletCodeValidator(string pattern)
        {
            _pattern = pattern;
        }

        public bool IsWellFormed(string palletCode, out string msg)
        {
            if (Regex.IsMatch(palletCode, _pattern))
            {
                msg = "OK";
                return true;
            }
            msg = $@"与模式不匹配 {_pattern}";
            return false;
        }
    }

}