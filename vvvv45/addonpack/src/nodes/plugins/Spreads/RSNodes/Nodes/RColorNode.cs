using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.Lib;
using VVVV.Utils.VColor;

namespace VVVV.Nodes
{

    public class RColorNode : IPlugin, IDisposable
    {
        #region Plugin Info
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "R";							//use CamelCaps and no spaces
                Info.Category = "Color";						//try to use an existing one
                Info.Version = "Advanced";						//versions are optional. leave blank if not needed
                Info.Help = "LTP version of R (Color)";
                Info.Bugs = "";
                Info.Credits = "";								//give credits to thirdparty code used
                Info.Warnings = "";
				Info.Author = "vux";

                //leave below as is
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
                System.Diagnostics.StackFrame sf = st.GetFrame(0);
                System.Reflection.MethodBase method = sf.GetMethod();
                Info.Namespace = method.DeclaringType.Namespace;
                Info.Class = method.DeclaringType.Name;
                return Info;
                //leave above as is
            }
        }
        #endregion

        #region Fields
        private IPluginHost FHost;
        private ColorDataHolder FData;

		private IValueConfig FPinDisplayChannel;
        private IStringIn FPinInReceiveString;
        private IColorOut FPinOutValue;
        private IColorIn FPinInDefault;
        private IValueOut FPinOutMatchCount;
        private string FKey = "";

        private bool FInvalidate = false;

        #endregion

        #region Auto Evaluate
        public bool AutoEvaluate
        {
            get { return true; }
        }
        #endregion

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            //assign host
            this.FHost = Host;
            this.FData = ColorDataHolder.Instance;
            this.FData.OnAdd += FData_OnAdd;
            this.FData.OnRemove += this.FData_OnAdd;
            this.FData.OnUpdate += this.FData_OnAdd;
			
			this.FHost.CreateValueConfig("Display Channel", 1, null, TSliceMode.Single, TPinVisibility.OnlyInspector, out this.FPinDisplayChannel);
			this.FPinDisplayChannel.SetSubType(0, 1, 1, 1, false, true, false);
    
            this.FHost.CreateStringInput("Receive String", TSliceMode.Single, TPinVisibility.True, out this.FPinInReceiveString);
            this.FPinInReceiveString.SetSubType("send", false);

            this.FHost.CreateColorInput("Default",TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInDefault);
            this.FPinInDefault.SetSubType(VColor.Black, true);

            this.FHost.CreateColorOutput("Output",TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutValue);
            this.FPinOutValue.SetSubType(VColor.Black, true);

            this.FHost.CreateValueOutput("Found", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinOutMatchCount);
            this.FPinOutMatchCount.SetSubType(0, 1, 1, 0, false, true, false);
        
        }

        void FData_OnAdd(string key)
        {
            if (this.FKey == key)
            {
                this.FInvalidate = true;
            }
        }
        #endregion

        #region Configurate
        public void Configurate(IPluginConfig Input)
        {
        }
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            if (this.FPinInReceiveString.PinIsChanged)
            {
                this.FPinInReceiveString.GetString(0, out this.FKey);
                if (this.FKey == null)
                {
                    this.FKey = "";
                }
                this.FInvalidate = true;
            }

            if (this.FInvalidate || this.FPinInDefault.PinIsChanged)
            {
                bool found;
                List<RGBAColor> dbl = this.FData.GetData(this.FKey,out found);
                if (found)
                {
                    this.FPinOutValue.SliceCount = dbl.Count;
                    for (int i = 0; i < dbl.Count; i++)
                    {
                        this.FPinOutValue.SetColor(i, dbl[i]);
                    }
                }
                else
                {
                    this.FPinOutValue.SliceCount = this.FPinInDefault.SliceCount;
                    for (int i = 0; i < this.FPinInDefault.SliceCount; i++)
                    {
                        RGBAColor col;
                        this.FPinInDefault.GetColor(i, out col);
                        this.FPinOutValue.SetColor(i, col);
                    }
                }
                this.FPinOutMatchCount.SetValue(0, Convert.ToDouble(found));
                this.FInvalidate = false;
            }

        }
        #endregion

        #region Dispose
        public void Dispose()
        {
        }
        #endregion
    }
        
        
}
