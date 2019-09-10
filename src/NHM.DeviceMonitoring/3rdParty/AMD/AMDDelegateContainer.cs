using System;

namespace ATI.ADL
{
    internal static partial class ADL
    {
        internal class AMDDelegateContainer<T> where T : Delegate
        {
            public AMDDelegateContainer(string funcName, T nativeDelegate)
            {
                _functionName = funcName;
                _nativeDelegate = nativeDelegate;
            }

            public T Delegate
            {
                get
                {
                    if (!_delegateCheck)
                    {
                        _delegateCheck = true;
                        _isFunctionValid = ADLCheckLibrary.IsFunctionValid(_functionName);
                    }
                    if (_isFunctionValid) return _nativeDelegate;
                    // function not found or not valid
                    return null;
                }
            }
            private string _functionName;
            private T _nativeDelegate;
            private bool _delegateCheck = false;
            private bool _isFunctionValid = false;
        }
    }
}
