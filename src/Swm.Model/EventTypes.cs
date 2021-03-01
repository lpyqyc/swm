// Copyright 2020-2021 王建军
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Swm.Model
{
    public static class EventTypes
    {
        public const string PreRequest = "PreRequest";

        public const string Request = "Request";

        public const string TaskCompleted = "TaskCompleted";

        public const string KeyPointChanged = "KeyPointChanged";

        public const string LocationInboundDisabled = "LocationInboundDisabled";

        public const string LocationInboundEnabled = "LocationInboundEnabled";

        public const string InboundOrderClosed = "InboundOrderClosed";

        public const string OutboundOrderClosed = "OutboundOrderClosed";
    }
}