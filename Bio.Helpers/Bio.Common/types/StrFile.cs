namespace Bio.Helpers.Common.Types {
	using System;
	using System.IO;

	public class StrFile:DisposableObject{
		
		private String FFileName = null;
		private System.Text.Encoding FEncode = null;
		private FileStream FStream = null;
		private StreamWriter FStreamWr = null;
		private StreamReader FStreamRd = null;
		private bool FIsWritable = false;

		public StrFile(String pFileName, System.Text.Encoding pEncode){
			this.FFileName = pFileName;
			this.FEncode = pEncode;
		}

		protected override void doOnDispose(){
			try{
				this.Close();
			}catch{}
		}

    public static void LoadStringFromFile(String pFileName, ref String vString, System.Text.Encoding pEncode){
      vString = null;
      if(File.Exists(pFileName)){
        System.Text.Encoding vEncode = pEncode;
        if(vEncode == null)
          vEncode = Utl.DefaultEncoding;
        FileStream fs = new FileStream(pFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        try {
          StreamReader vFile = new StreamReader(fs, vEncode);
          String line;
          StringWriter sw = new StringWriter();
          while((line = vFile.ReadLine()) != null) {
            sw.WriteLine(line);
          }
          vString = sw.ToString();
        } finally {
          fs.Close();
        }
      }
    }

    public static void SaveStringToFile(String pFileName, String vString, System.Text.Encoding pEncode){
      if(File.Exists(pFileName))
        File.Delete(pFileName);
      FileStream fs = new FileStream(pFileName, FileMode.CreateNew, FileAccess.Write, FileShare.None);
      System.Text.Encoding vEncode = pEncode;
      if(vEncode == null)
        vEncode = Utl.DefaultEncoding;
      using (StreamWriter sw = new StreamWriter(fs, vEncode)){
        sw.WriteLine(vString);
        sw.Flush();
        sw.Close();
      }
    }

		public void OpenForWrite(){
			this.FStream = new FileStream(this.FFileName, FileMode.Append, FileAccess.Write, FileShare.Read);
			System.Text.Encoding vEncode = this.FEncode;
			if(vEncode == null)
        vEncode = Utl.DefaultEncoding;
			this.FStreamWr = new StreamWriter(this.FStream, vEncode);
			this.FIsWritable = true;
		}

		public String FileName{
			get{
				return this.FFileName;
			}
		}

		public void Rename(String pDestPath){
			lock(this){
        if(pDestPath != this.FFileName){
          if(File.Exists(this.FileName)){
            String vDestPath = Path.GetFullPath(pDestPath);
            if(!Directory.Exists(vDestPath))
              Directory.CreateDirectory(Path.GetDirectoryName(vDestPath));
            if(File.Exists(pDestPath))
              File.Delete(pDestPath);
            File.Move(this.FileName, pDestPath);
          }
          this.FFileName = pDestPath;
        }
			}
		}

		public void OpenAddLineClose(String pLine){
			lock(this){
				String vPath = Path.GetDirectoryName(this.FFileName);
				if(!Directory.Exists(vPath))
					Directory.CreateDirectory(vPath);
				FileStream vStream = new FileStream(this.FFileName, FileMode.Append, FileAccess.Write, FileShare.Read);
				System.Text.Encoding vEncode = this.FEncode;
				if(vEncode == null)
          vEncode = Utl.DefaultEncoding;
				StreamWriter vStreamWr = new StreamWriter(vStream, vEncode);
				vStreamWr.WriteLine(pLine);
				if(vStreamWr != null){
					vStreamWr.Flush();
					vStreamWr.Close();
				}
				if(vStream != null)
					vStream.Close();
			}
		}

		public void OpenAddTimedLineClose(String pLine){
			this.OpenAddLineClose(DateTime.Now.ToString("yyyy.MM.dd hh:mm:ss")+" : "+pLine);
		}

		public void AddLine(String pLine){
			if(this.FIsWritable)
				this.FStreamWr.WriteLine(pLine);
		}

		public void OpenForRead(){
			this.FIsWritable = false;
			if(File.Exists(this.FFileName)){
				System.Text.Encoding vEncode = this.FEncode;
				if(vEncode == null)
          vEncode = Utl.DefaultEncoding;
				this.FStream = new FileStream(this.FFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
				this.FStreamRd = new StreamReader(this.FStream, vEncode);
			}
		}

		public String GetLine(){
			if(!this.FIsWritable)
				return this.FStreamRd.ReadLine();
			else
				return null;
		}

		public void Close(){
			if(this.FIsWritable){
				if(this.FStreamWr != null){
					this.FStreamWr.Flush();
					this.FStreamWr.Close();
				}
			}else{
				if(this.FStreamRd != null)
					this.FStreamRd.Close();
			}
			if(this.FStream != null)
				this.FStream.Close();
		}
	}
}
