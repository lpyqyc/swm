using Arctic.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Swm.Model
{
    public class Port : IHasCtime
    {
        public Port()
        {
            this.ctime = DateTime.Now;
            this.Laneways = new HashSet<Laneway>();
        }

        public virtual Int32 PortId { get; internal protected set; }

        [Required]
        [MaxLength(20)]
        public virtual string PortCode { get; set; }

        public virtual Int32 v { get; set; }

        public virtual DateTime ctime { get; set; }

        [Required]
        public virtual Location KP1 { get; set; }

        public virtual Location KP2 { get; set; }

        public virtual String Comment { get; set; }


        public virtual ISet<Laneway> Laneways { get; protected set; }

        // TODO 
        //public virtual IUnitloadAllocationTable CurrentUat { get; protected set; }

        public virtual DateTime CheckedAt { get; set; }

        public virtual String CheckMessage { get; set; }


        //public virtual void SetCurrentUat(IUnitloadAllocationTable uat)
        //{
        //    if (uat == null)
        //    {
        //        throw new ArgumentNullException(nameof(uat));
        //    }

        //    if (this.CurrentUat == uat)
        //    {
        //        return;
        //    }

        //    if (this.CurrentUat != null)
        //    {
        //        throw new InvalidOperationException($"出口已被占用。【{this.CurrentUat}】");
        //    }

        //    this.CurrentUat = uat;
        //}

        // TODO 
        //public virtual void ResetCurrentUat()
        //{
        //    this.CurrentUat = null;
        //}

        public override string ToString()
        {
            return this.PortCode;
        }

    }

}
