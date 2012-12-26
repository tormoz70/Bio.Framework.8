namespace Bio.Helpers.Common.Types {
  using System;

	public class CDisposableObject: IDisposable{
		private bool FIsDisposed = false;

		protected virtual void OnDispose(){}
		private void Dispose(bool disposing){
			if(!FIsDisposed){
				if(disposing){
					OnDispose();
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
