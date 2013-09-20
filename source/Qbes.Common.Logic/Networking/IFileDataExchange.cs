using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qbes.Common.Logic.Networking
{
   /// <summary>
   /// This common interface is used for file data exchange between client and
   /// server.
   /// </summary>
   public interface IFileDataExchange
   {
      /// <summary>
      /// One-way notification to add file to cache.
      /// </summary>
      /// <param name="connection">Connection object</param>
      /// <param name="exchangeType">Exchange type</param>
      /// <param name="hash">Content checksum</param>
      /// <param name="data">File data</param>
      void AddFile(Connection connection, byte exchangeType, byte[] hash, byte[] data);

      /// <summary>
      /// Request to check whether a local file exists on the other side.
      /// </summary>
      /// <param name="connection">Connection object</param>
      /// <param name="exchangeType">Exchange type</param>
      /// <param name="hash">Content checksum</param>
      void CheckFileExistRequest(Connection connection, byte exchangeType, byte[] hash);

      /// <summary>
      /// Response to check request.
      /// </summary>
      /// <param name="connection">Connection object</param>
      /// <param name="exchangeType">Exchange type</param>
      /// <param name="hash">Content checksum</param>
      /// <param name="exists">True if exists</param>
      void CheckFileExistResponse(Connection connection, byte exchangeType, byte[] hash, bool exists);

      /// <summary>
      /// Request a specific file.
      /// </summary>
      /// <param name="connection">Connection object</param>
      /// <param name="exchangeType">Exchange type</param>
      /// <param name="hash">Content checksum</param>
      void RequestFile(Connection connection, byte exchangeType, byte[] hash);
   }
}
