﻿#region license
/**
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

/** Modifications copyright(C) 2020 Adrian Strugała **/
#endregion

namespace SolTechnology.Avro.Constants
{
    internal class DataFileConstants
    {
        internal const string SyncMetadataKey = "avro.sync";
        internal const string CodecMetadataKey = "avro.codec";
        internal const string SchemaMetadataKey = "avro.schema";
        internal const string MetaDataReserved = "avro";

        internal static byte[] AvroHeader = { (byte)'O',
                                            (byte)'b',
                                            (byte)'j',
                                            (byte) 1 };

        internal const int SyncSize = 16;
        internal const int DefaultSyncInterval = 4000 * SyncSize;
    }
}
