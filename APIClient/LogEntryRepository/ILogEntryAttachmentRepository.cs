﻿using Data.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunarAPIClient.LogEntryRepository
{
    public interface ILogEntryAttachmentRepository
    {
        /// <summary>
        /// Returns the number of blocks that have completed their uploads
        /// </summary>
        /// <param name="missionId"></param>
        /// <param name="logEntryId"></param>
        /// <param name="binaryPayloadValue"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<bool> UploadAttachmentPart(Guid missionId, Guid logEntryId, BinaryPayloadValue binaryPayloadValue, string contentType, CancellationToken cancellationToken);
    }
}
