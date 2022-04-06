using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.UI
{
    public abstract class InputParameter
    {
        public enum ParamType
        {
            Bool,
            String,
            Decimal,
            Integer,
            Dropdown,
        }

        public ParamType paramType { get; private set; } = ParamType.String;

        public string paramName { get; private set; }

        public InputParameter(ParamType paramType, string paramName)
        {
            this.paramType = paramType;
            this.paramName = paramName;
        }
    }

    public class BoolParam : InputParameter
    {
        public bool defaultValue { get; private set; } = false;
        public bool value { get; set; } = false;

        public BoolParam(string paramName, bool defaultValue)
            : base(ParamType.Bool, paramName)
        {
            this.defaultValue = defaultValue;
            value = this.defaultValue;
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }

    public class StringParam : InputParameter
    {
        public string defaultValue { get; private set; } = "";
        public string value { get; set; } = "";

        public StringParam(string paramName, string defaultValue)
            : base(ParamType.String, paramName)
        {
            this.defaultValue = defaultValue;
            value = this.defaultValue;
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }

    public class DecimalParam : InputParameter
    {
        public float defaultValue { get; private set; } = 0.0F;
        public float value { get; set; } = 0.0F;

        public float minValue { get; private set; } = 0.0F;
        public float maxValue { get; private set; } = 1.0F;

        public DecimalParam(string paramName, float defaultValue, float minValue, float maxValue)
            : base(ParamType.Decimal, paramName)
        {
            this.defaultValue = defaultValue;
            value = this.defaultValue;
            this.minValue = minValue;
            this.maxValue = maxValue;
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }

    public class IntegerParam : InputParameter
    {
        public int defaultValue { get; private set; } = 0;
        public int value { get; set; } = 0;

        public int minValue { get; private set; } = 0;
        public int maxValue { get; private set; } = 1;

        public IntegerParam(string paramName, int defaultValue, int minValue, int maxValue)
            : base(ParamType.Integer, paramName)
        {
            this.defaultValue = defaultValue;
            value = this.defaultValue;
            this.minValue = minValue;
            this.maxValue = maxValue;
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }

    public class DropdownParam : InputParameter
    {
        public int defaultValue { get; private set; } = 0;
        public int value { get; set; } = 0;

        public List<Dropdown.OptionData> options { get; private set; } = null;

        public DropdownParam(string paramName, int defaultValue, List<Dropdown.OptionData> options)
            : base(ParamType.Dropdown, paramName)
        {
            this.defaultValue = defaultValue;
            value = this.defaultValue;
            this.options = options;
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }

    public class ParameterCollection
    {
        public Dictionary<string, InputParameter> parameters { get; private set; } = null;

        public ParameterCollection()
        {
            parameters = new Dictionary<string, InputParameter>();
        }

        public bool AddParam(InputParameter roundParam)
        {
            if(!string.IsNullOrWhiteSpace(roundParam.paramName))
            {
                if(!parameters.ContainsKey(roundParam.paramName))
                {
                    parameters.Add(roundParam.paramName, roundParam);
                    return true;
                }
                else
                {
                    CoinModeLogging.LogWarning("ParameterCollection", "AddParam", "Cannot add param to collection, param name is already in use!");
                }
            }
            else
            {
                CoinModeLogging.LogWarning("ParameterCollection", "AddParam", "Cannot add param to collection, name is not set!");
            }
            return true;
        }

        public bool UpdateParamValue(string paramName, string value)
        {
            InputParameter roundParam = null;
            if(parameters.TryGetValue(paramName, out roundParam))
            {
                if(roundParam.paramType != InputParameter.ParamType.String)
                {
                    StringParam stringParam = roundParam as StringParam;
                    stringParam.value = value;
                    return true;
                }
                else
                {
                    CoinModeLogging.LogWarning("ParameterCollection", "UpdateParam", "Param {0} is type {1}, attempted to set value with string!",
                        paramName);
                }
            }
            else
            {
                CoinModeLogging.LogWarning("ParameterCollection", "UpdateParam", "Cannot update round param, param not found with name {0}!",
                    paramName);
            }
            return false;
        }

        public bool UpdateParamValue(string paramName, bool value)
        {
            InputParameter roundParam = null;
            if (parameters.TryGetValue(paramName, out roundParam))
            {
                if (roundParam.paramType != InputParameter.ParamType.Bool)
                {
                    BoolParam boolParam = roundParam as BoolParam;
                    boolParam.value = value;
                    return true;
                }
                else
                {
                    CoinModeLogging.LogWarning("ParameterCollection", "UpdateParam", "Param {0} is type {1}, attempted to set value with bool!",
                        paramName);
                }
            }
            else
            {
                CoinModeLogging.LogWarning("ParameterCollection", "UpdateParam", "Cannot update round param, param not found with name {0}!",
                    paramName);
            }
            return false;
        }

        public bool UpdateParamValue(string paramName, float value)
        {
            InputParameter roundParam = null;
            if (parameters.TryGetValue(paramName, out roundParam))
            {
                if (roundParam.paramType != InputParameter.ParamType.Decimal)
                {
                    DecimalParam decimalParam = roundParam as DecimalParam;
                    decimalParam.value = value;
                    return true;

                }
                else
                {
                    CoinModeLogging.LogWarning("ParameterCollection", "UpdateParam", "Param {0} is type {1}, attempted to set value with float!",
                        paramName);
                }
            }
            else
            {
                CoinModeLogging.LogWarning("ParameterCollection", "UpdateParam", "Cannot update round param, param not found with name {0}!",
                    paramName);
            }
            return false;
        }

        public bool UpdateParamValue(string paramName, int value)
        {
            InputParameter roundParam = null;
            if (parameters.TryGetValue(paramName, out roundParam))
            {
                if (roundParam.paramType != InputParameter.ParamType.Integer && roundParam.paramType != InputParameter.ParamType.Dropdown)
                {
                    switch (roundParam.paramType)
                    {
                        case InputParameter.ParamType.Integer:
                            {
                                IntegerParam integerParam = roundParam as IntegerParam;
                                integerParam.value = value;
                            }
                            break;
                        case InputParameter.ParamType.Dropdown:
                            {
                                DropdownParam dropdownParam = roundParam as DropdownParam;
                                dropdownParam.value = value;
                            }
                            break;
                    }                    
                    return true;
                }
                else
                {
                    CoinModeLogging.LogWarning("ParameterCollection", "UpdateParam", "Param {0} is type {1}, attempted to set value with int!",
                        paramName);
                }
            }
            else
            {
                CoinModeLogging.LogWarning("ParameterCollection", "UpdateParam", "Cannot update round param, param not found with name {0}!",
                    paramName);
            }
            return false;
        }
    }
}
