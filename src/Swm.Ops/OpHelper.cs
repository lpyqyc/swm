using Swm.Model;
using System.Threading.Tasks;

namespace Swm.Web
{
    /// <summary>
    /// 提供操作记录相关的方法
    /// </summary>
    internal class OpHelper
    {
        NHibernate.ISession _session;

        /// <summary>
        /// 初始化新实例
        /// </summary>
        /// <param name="session"></param>
        public OpHelper(NHibernate.ISession session)
        {
            _session = session;
        }

        /// <summary>
        /// 创建一个 Op 对象并保存到数据库。
        /// </summary>
        /// <param name="format">用于填充 <see cref="Op.Comment"/> 属性的格式化字符串。</param>
        /// <param name="args">用于填充 <see cref="Op.Comment"/> 属性的格式化参数。</param>
        /// <returns></returns>
        public async Task<Op> SaveOpAsync(string operationType, string displayUrl, string format, params object[] args)
        {
            Op op = new Op();

            op.OperationType = operationType;
            op.Comment = string.Format(format, args);
            op.Url = displayUrl;
            await _session.SaveAsync(op);

            return op;
        }

    }

}