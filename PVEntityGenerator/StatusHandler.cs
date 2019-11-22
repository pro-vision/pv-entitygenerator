using System;
using System.Windows.Forms;

namespace PVEntityGenerator {

  public class StatusHandler {

    private int mMaxValue = 0;

    public delegate void StatusEventHandler(object pSender, StatusHandlerEventArgs pArgs);
    public event StatusEventHandler Status;

    public delegate void ErrorEventHandler(object pSender, ErrorHandlerEventArgs pArgs);
    public event ErrorEventHandler Error;

    public DialogResult OnError(string pError) {
      if (this.Error!=null) {
        ErrorHandlerEventArgs args=new ErrorHandlerEventArgs(pError);
        this.Error(this,args);
        return args.Result;
      } else {
        return DialogResult.Cancel;
      }
    }

    public void InitStatus(string pStatus, int pMaxValue) {
      mMaxValue = pMaxValue;
      if (this.Status!=null) {
        this.Status(this, new StatusHandlerEventArgs(pStatus, 0, pMaxValue));
      }
    }

    public void SetStatus(string pStatus, int pActualValue) {
      if (this.Status!=null) {
        this.Status(this, new StatusHandlerEventArgs(pStatus, pActualValue, (pActualValue > mMaxValue) ? pActualValue : mMaxValue));
      }
    }

    public void ClearStatus(string pStatus) {
      if (this.Status!=null) {
        this.Status(this, new StatusHandlerEventArgs(pStatus, 0, 0));
      }
    }

    public void ClearStatus() {
      this.ClearStatus("");
    }

    public class ErrorHandlerEventArgs : System.EventArgs {
      string mErr;
      DialogResult mResult;

      internal ErrorHandlerEventArgs(string pErrorDescription) {
        mErr=pErrorDescription;
        mResult=DialogResult.Cancel;
      }

      public string ErrorDescription {
        get { return mErr; }
      }

      public DialogResult Result {
        get { return mResult; }
        set { mResult=value; }
      }
    }

    public class StatusHandlerEventArgs : System.EventArgs {
      private string mStatus = null;
      private int mActualValue = 0;
      private int mMaxValue = 0;

      internal StatusHandlerEventArgs(string pStatus, int pActualValue, int pMaxValue) {
        mStatus = pStatus;
        mActualValue = pActualValue;
        mMaxValue = pMaxValue;
      }

      public string Status {
        get {
          return mStatus;
        }
      }

      public int ActualValue {
        get {
          return mActualValue;
        }
      }

      public int MaxValue {
        get {
          return mMaxValue;
        }
      }

    }

  }

}
