namespace Bio.Helpers.Common.Types {
  using System;

	/// <summary>
	/// ”ничтожаемый объект
	/// </summary>
	public class DisposableObject: IDisposable{
		private Boolean _isDisposed;

		protected virtual void doOnDispose(){}
		private void Dispose(Boolean disposing){
			if(!this._isDisposed){
				if(disposing){
					doOnDispose();
				}
			}
      this._isDisposed = true;
		}

		public void Dispose(){
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		
		~DisposableObject(){
			this.Dispose(false);
		}
		
	}
}
