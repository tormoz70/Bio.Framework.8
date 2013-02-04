namespace Bio.Helpers.Common.Types {
  using System;

	public class CDisposableObject: IDisposable{
		private bool FIsDisposed = false;

		protected virtual void onDispose(){}
		private void Dispose(bool disposing){
			if(!FIsDisposed){
				if(disposing){
					onDispose();
				}
			}
			FIsDisposed = true;
		}

		public void Dispose(){
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		
		~CDisposableObject(){
			this.Dispose(false);
		}
		
	}
}
