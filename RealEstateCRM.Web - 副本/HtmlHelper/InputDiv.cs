using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RealEstateCRM.Web
{
    public class InputDiv:IDisposable
    {
         private readonly ViewContext _viewContext;
        private bool _disposed;
        public InputDiv(ViewContext viewContext)
        {
            if (viewContext == null)
            {
                throw new ArgumentNullException("viewContext");
            }

            _viewContext = viewContext;

            // push the new FormContext
            _viewContext.FormContext = new FormContext();
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true /* disposing */);
            GC.SuppressFinalize(this);
            
        }
    }
}