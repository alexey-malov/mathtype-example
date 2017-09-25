using MTSDKDN;
using System;
using System.Text;

namespace ConsoleApp1
{
    public class MathTypeSdkFacade
    {
        private const short INITIALIZATION_TIMEOUT = 300; // in seconds
        private const int INITIAL_RESULT_BUFFER_SIZE = 100 * 1024; // 100 KB
        private const int MAX_RESULT_BUFFER_SIZE = INITIAL_RESULT_BUFFER_SIZE * 20; // 2 MB

        private bool initialized = false;

        private MathTypeSDK Api {
            get
            {
                return MathTypeSDK.Instance;
            }
        }

        public MathTypeSdkFacade()
        {

        }

        ~MathTypeSdkFacade()
        {
            Disconnect();
        }

        public string ConvertEquation(byte[] equationRawBytes, string outputTranslator)
        {
            Connect();
            try
            {
                SetTranslator(outputTranslator);
                return ConvertEquationImpl(equationRawBytes);
            }
            finally
            {
                Disconnect();
            }
        }

        private string ConvertEquationImpl(byte[] equationRawBytes)
        {
            MTAPI_DIMS dims = new MTAPI_DIMS();
            var resultStringBuilder = new StringBuilder(INITIAL_RESULT_BUFFER_SIZE);
            var success = false;
            while (!success)
            {
                if (resultStringBuilder.Capacity > MAX_RESULT_BUFFER_SIZE)
                {
                    throw new Exception("Could not convert due high memory usage");
                }

                int result;
                try
                {
                    result = Api.MTXFormEqnMgn(
                        MTXFormEqn.mtxfmLOCAL,
                        MTXFormEqn.mtxfmMTEF,
                        equationRawBytes,
                        equationRawBytes.Length,
                        MTXFormEqn.mtxfmLOCAL,
                        MTXFormEqn.mtxfmTEXT,
                        resultStringBuilder,
                        resultStringBuilder.Capacity,
                        string.Empty,
                        ref dims
                    );
                }
                catch (AccessViolationException e)
                {
                    throw new Exception("Access violation exception while FormEqn." + e.ToString());
                }
                
                success = MathTypeReturnValue.mtOK == result;
                if (MathTypeReturnValue.mtMEMORY == result)
                {
                    resultStringBuilder.Clear();
                    resultStringBuilder.Capacity *= 2; // retry with increased buffer
                }
                else if (MathTypeReturnValue.mtTRANSLATOR_ERROR == result)
                {
                    throw new Exception("mtTRANSLATOR_ERROR. Result eq: " + resultStringBuilder.ToString());
                }
                else if (!success)
                {
                    throw new Exception("MathType returned: " + result);
                }
            }
            return resultStringBuilder.ToString();
        }

        private bool Connect()
        {
            if (initialized)
            {
                return true;
            }

            int result = Api.MTAPIConnectMgn(MTApiStartValues.mtinitLAUNCH_AS_NEEDED, INITIALIZATION_TIMEOUT);
            initialized = (result == MathTypeReturnValue.mtOK);
            if (!initialized)
            {
                throw new Exception("Can't connect to MathType. Returned: " + result);
            }
            return initialized;
        }

        private void Disconnect()
        {
            if (initialized)
            {
                initialized = false;
                Api.MTAPIDisconnectMgn();
            }
        }

        private bool ResetForm()
        {
            return MathTypeReturnValue.mtOK == Api.MTXFormResetMgn();
        }

        private void SetTranslator(string translator)
        {
            var result = Api.MTXFormSetTranslatorMgn((ushort)MTXFormSetTranslator.mtxfmTRANSL_INC_NONE, translator);
            var success = MathTypeReturnValue.mtOK == result;
            if (!success)
            {
                throw new Exception("Can't set translator: returned " + result);
            }
        }
    }
}