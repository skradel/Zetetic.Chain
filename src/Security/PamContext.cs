using System;
using System.Collections.Generic;
using System.Text;
using Zetetic.Chain;
using System.Security.Principal;

namespace Zetetic.Chain.Security
{
    public class PamContext : ContextBase
    {
        private IPrincipal _principal;
        private object _target;
        private string _operation;
        private bool _permit, _permDenied;

        public IPrincipal Principal
        {
            get
            {
                return _principal;
            }

            internal set
            {
                _principal = value;
                this["Principal"] = value;
            }
        }

        public string Operation
        {
            get
            {
                return _operation;
            }
            protected set
            {
                this["Operation"] = value;
                _operation = value;
            }
        }

        public object Target
        {
            get
            {
                return _target;
            }

            internal set
            {
                _target = value;
                this["Target"] = value;
            }
        }

        public void MarkDenied()
        {
            _permDenied = true;
        }

        /// <summary>
        /// The ultimate decision of permitted-ness.  This should only be set from PamCommand.AproposResponse.
        /// </summary>
        public bool Permit
        {
            get
            {
                return _permit;
            }

            internal set
            {
                if (!_permDenied)
                {
                    _permit = value;
                    this["Permit"] = value;
                }
            }
        }


        public PamContext(IPrincipal principal, string operation, object target)
            : base()
        {
            this.Principal = principal;
            this.Operation = operation;
            this.Target = target;
        }
    }
}
