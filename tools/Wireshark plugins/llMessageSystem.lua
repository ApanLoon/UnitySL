
-- Dissector for Linden Labs UDP message protocol
--
-- Messages are described here:
--   http://wiki.secondlife.com/wiki/Packet_Layout
--   http://wiki.secondlife.com/wiki/Message_Layout
--   https://bitbucket.org/lindenlab/viewer/src/04c473ab46041133ea6a87dbe0d43e662472adf5/scripts/messages/message_template.msg
--
-- Install:
--   Copy this file to one of the locations Wireshark uses to load plugins.
--   If you go to Help –> About Wireshark –> Folders, you’ll find all the
--   folders Wireshark reads Lua scripts from. Choose either the Personal
--   Lua Plugins, Global Lua Plugins or Personal configuration folder. E.g.
--   C:\Program Files\Wireshark\plugins\3.4 on Windows. The script will be
--   active when Wireshark is started. You have to restart Wireshark after
--   you do changes to the script, or reload all the Lua scripts with
--   Ctrl+Shift+L.
--
-- For debugging, it can be nice to force the lua console to stay open on
-- reload, to do this make sure that the last thing done in
--   C:\Program Files\Wireshark\console.lua
-- is 
--   run_console()
--
first = true

llmsg_protocol = Proto("llMsg",  "LL Message System Protocol")

Header = {}
Body = {}
BodyTree = {}

-- Helpers
function HeaderFlagsToString(flags)
	local name = ""
	if bitand(flags, 0x80) ~= 0 then name = name .. " ZERO_CODE" end
	if bitand(flags, 0x40) ~= 0 then name = name .. " RELIABLE" end
	if bitand(flags, 0x20) ~= 0 then name = name .. " RESENT" end
	if bitand(flags, 0x10) ~= 0 then name = name .. " ACK" end
	return trim(name)
end

function GetMessageIdString(name, messageNumber)
	local number
	local frequency
	    if messageNumber < 0xff       then frequency = "High";   number = bitand(messageNumber, 0xff)
	elseif messageNumber < 0xffff0000 then frequency = "Medium"; number = bitand(messageNumber, 0xff)
	elseif messageNumber < 0xfffffffa then frequency = "Low";    number = bitand(messageNumber, 0xffff)
	else                                   frequency = "Fixed";  number = string.format("0x%04x", messageNumber)
	end
	return string.format("%s %s %s", name, frequency, number)
end

function ChatTypeToString(type)
	    if type == 0 then return "whisper"
	elseif type == 1 then return "normal"
	elseif type == 2 then return "shout"
	elseif type == 3 then return "n/a"
	elseif type == 4 then return "start"
	elseif type == 5 then return "stop"
	elseif type == 6 then return "debug"
	elseif type == 7 then return "region"
	elseif type == 8 then return "owner"
	elseif type == 9 then return "direct"
	else return "unknown"
	end
end

function TransactionTypeToString(transactionType)
	if transactionType == 0 then return "balance update"
	-- Codes 1000-1999 reserved for one-time charges
	elseif transactionType == 1000 then return "object claim"
	elseif transactionType == 1001 then return "land claim"
	elseif transactionType == 1002 then return "group create"
	elseif transactionType == 1003 then return "object public claim"
	elseif transactionType == 1004 then return "group join"
	elseif transactionType == 1100 then return "teleport charge"
	elseif transactionType == 1101 then return "upload charge"
	elseif transactionType == 1102 then return "land auction"
	elseif transactionType == 1103 then return "classified charge"

	-- Codes 2000-2999 reserved for recurrent charges
	elseif transactionType == 2000 then return "object tax"
	elseif transactionType == 2001 then return "land tax"
	elseif transactionType == 2002 then return "light tax"
	elseif transactionType == 2003 then return "parcel dir fee"
	elseif transactionType == 2004 then return "group tax"
	elseif transactionType == 2005 then return "classified renew"

	-- Codes 2100-2999 reserved for recurring billing services
	-- New codes can be created through an admin interface so may not
	-- automatically end up in the list below :-(
	-- So make sure you check the transaction_description table
	elseif transactionType == 2100 then return "recurring generic"

	-- Codes 3000-3999 reserved for inventory transactions
	elseif transactionType == 3000 then return "give inventory"

	-- Codes 5000-5999 reserved for transfers between users
	elseif transactionType == 5000 then return "object sale"
	elseif transactionType == 5001 then return "gift"
	elseif transactionType == 5002 then return "land sale"
	elseif transactionType == 5003 then return "refer bonus"
	elseif transactionType == 5004 then return "inventory sale"
	elseif transactionType == 5005 then return "refund purchase"
	elseif transactionType == 5006 then return "land pass sale"
	elseif transactionType == 5007 then return "dwell bonus"
	elseif transactionType == 5008 then return "pay object"
	elseif transactionType == 5009 then return "object pays"

	-- Codes 5100-5999 reserved for recurring billing transfers between users
	-- New codes can be created through an admin interface so may not
	-- automatically end up in the list below :-(
	-- So make sure you check the transaction_description table
	elseif transactionType == 5100 then return "recurring generic user"

	-- Codes 6000-6999 reserved for group transactions
	elseif transactionType == 6000 then return "reserved for future use"
	elseif transactionType == 6001 then return "group land deed"
	elseif transactionType == 6002 then return "group object deed"
	elseif transactionType == 6003 then return "group liability"
	elseif transactionType == 6004 then return "group dividend"
	elseif transactionType == 6005 then return "membership dues"

	-- Codes 8000-8999 reserved for one-type credits
	elseif transactionType == 8000 then return "object release"
	elseif transactionType == 8001 then return "land release"
	elseif transactionType == 8002 then return "object delete"
	elseif transactionType == 8003 then return "object public decay"
	elseif transactionType == 8004 then return "object public delete"

	-- Code 9000-9099 reserved for usertool transactions
	elseif transactionType == 9000 then return "linden adjustment"
	elseif transactionType == 9001 then return "linden grant"
	elseif transactionType == 9002 then return "linden penalty"
	elseif transactionType == 9003 then return "event fee"
	elseif transactionType == 9004 then return "event prize"

	-- These must match entries in money_stipend table in MySQL
	-- Codes 10000-10999 reserved for stipend credits
	elseif transactionType == 10000 then return "stipend basic"
	elseif transactionType == 10000 then return "stipend developer"
	elseif transactionType == 10000 then return "stipend always"
	elseif transactionType == 10000 then return "stipend daily"
	elseif transactionType == 10000 then return "stipend rating"
	elseif transactionType == 10000 then return "stipend delta"
	
	else return "unknown"
	end
end

function RegionFlagsToString(flags)
	local name = ""
	if bitand(flags, 0x00000001) ~= 0 then name = name .. " ALLOW_DAMAGE" end
	if bitand(flags, 0x00000002) ~= 0 then name = name .. " ALLOW_LANDMARK" end
	if bitand(flags, 0x00000004) ~= 0 then name = name .. " ALLOW_SET_HOME" end
	if bitand(flags, 0x00000008) ~= 0 then name = name .. " RESET_HOME_ON_TELEPORT" end
	if bitand(flags, 0x00000010) ~= 0 then name = name .. " SUN_FIXED" end
	if bitand(flags, 0x00000020) ~= 0 then name = name .. " ALLOW_ACCESS_OVERRIDE" end
	if bitand(flags, 0x00000040) ~= 0 then name = name .. " BLOCK_TERRAFORM" end
	if bitand(flags, 0x00000080) ~= 0 then name = name .. " BLOCK_LAND_RESELL" end
	if bitand(flags, 0x00000100) ~= 0 then name = name .. " SANDBOX" end
	if bitand(flags, 0x00000200) ~= 0 then name = name .. " ALLOW_ENVIRONMENT_OVERRIDE" end

	if bitand(flags, 0x00001000) ~= 0 then name = name .. " SKIP_COLLISIONS" end
	if bitand(flags, 0x00002000) ~= 0 then name = name .. " SKIP_SCRIPTS" end
	if bitand(flags, 0x00004000) ~= 0 then name = name .. " SKIP_PHYSICS" end
	if bitand(flags, 0x00008000) ~= 0 then name = name .. " EXTERNALLY_VISIBLE" end
	if bitand(flags, 0x00010000) ~= 0 then name = name .. " ALLOW_RETURN_ENCROACHING_OBJECT" end
	if bitand(flags, 0x00020000) ~= 0 then name = name .. " ALLOW_RETURN_ENCROACHING_ESTATE_OBJECT" end
	if bitand(flags, 0x00040000) ~= 0 then name = name .. " BLOCK_DWELL" end
	if bitand(flags, 0x00080000) ~= 0 then name = name .. " BLOCK_FLY" end
	if bitand(flags, 0x00100000) ~= 0 then name = name .. " ALLOW_DIRECT_TELEPORT" end
	if bitand(flags, 0x00200000) ~= 0 then name = name .. " ESTATE_SKIP_SCRIPTS" end
	if bitand(flags, 0x00400000) ~= 0 then name = name .. " RESTRICT_PUSHOBJECT" end
	if bitand(flags, 0x00800000) ~= 0 then name = name .. " DENY_ANONYMOUS" end

	if bitand(flags, 0x04000000) ~= 0 then name = name .. " ALLOW_PARCEL_CHANGES" end
	if bitand(flags, 0x08000000) ~= 0 then name = name .. " BLOCK_FLYOVER" end
	if bitand(flags, 0x10000000) ~= 0 then name = name .. " ALLOW_VOICE" end
	if bitand(flags, 0x20000000) ~= 0 then name = name .. " BLOCK_PARCEL_SEARCH" end
	if bitand(flags, 0x40000000) ~= 0 then name = name .. " DENY_AGEUNVERIFIED" end
	return trim(name)
end

function GuidToString(buffer)
	local a = buffer(0, 4)
	local b = buffer(4, 2)
	local c = buffer(6, 2)
	local d = buffer(8, 2)
	local e = buffer(10, 6)
	return string.format("%s-%s-%s-%s-%s", a, b, c, d, e)
end


function AddFieldToTree(tree, field, buffer, start, count, text)
	local added = tree:add(field, buffer(start, count))
	if text then
		added:append_text(text)
	end
	return start + count
end

function AddFieldToTree_le(tree, field, buffer, start, count, text)
	local added = tree:add_le(field, buffer(start, count))
	if text then
		added:append_text(text)
	end
	return start + count
end

function AddVector3ToTree(tree, messageNumber, fieldPrefix, buffer, offset)
	local subtree = tree:add(llmsg_protocol, buffer(offset, 12), fieldPrefix)
	local x = buffer(offset, 4):le_float()
	offset = AddFieldToTree_le(subtree, Decoders[messageNumber].Fields[fieldPrefix .. "X"], buffer, offset,  4)
	local y = buffer(offset, 4):le_float()
	offset = AddFieldToTree_le(subtree, Decoders[messageNumber].Fields[fieldPrefix .. "Y"], buffer, offset,  4)
	local z = buffer(offset, 4):le_float()
	offset = AddFieldToTree_le(subtree, Decoders[messageNumber].Fields[fieldPrefix .. "Z"], buffer, offset,  4)
	subtree:append_text(string.format(": <%f, %f, %f>", x, y, z))
	return offset
end

-- Expected to always be unit quaternions, so w is not included
function AddQuaternionToTree(tree, messageNumber, fieldPrefix, buffer, offset)
	local subtree = tree:add(llmsg_protocol, buffer(offset, 12), fieldPrefix)
	local x = buffer(offset, 4):le_float()
	offset = AddFieldToTree_le(subtree, Decoders[messageNumber].Fields[fieldPrefix .. "X"], buffer, offset,  4)
	local y = buffer(offset, 4):le_float()
	offset = AddFieldToTree_le(subtree, Decoders[messageNumber].Fields[fieldPrefix .. "Y"], buffer, offset,  4)
	local z = buffer(offset, 4):le_float()
	offset = AddFieldToTree_le(subtree, Decoders[messageNumber].Fields[fieldPrefix .. "Z"], buffer, offset,  4)
	subtree:append_text(string.format(": <%f, %f, %f>", x, y, z))
	return offset
end

function AddColor4UToTree(tree, messageNumber, fieldPrefix, buffer, offset)
	local subtree = tree:add(llmsg_protocol, buffer(offset, 4), fieldPrefix)
	local r = buffer(offset, 1):uint()
	offset = AddFieldToTree_le(subtree, Decoders[messageNumber].Fields[fieldPrefix .. "R"], buffer, offset,  1)
	local g = buffer(offset, 1):uint()
	offset = AddFieldToTree_le(subtree, Decoders[messageNumber].Fields[fieldPrefix .. "G"], buffer, offset,  1)
	local b = buffer(offset, 1):uint()
	offset = AddFieldToTree_le(subtree, Decoders[messageNumber].Fields[fieldPrefix .. "B"], buffer, offset,  1)
	local a = buffer(offset, 1):uint()
	offset = AddFieldToTree_le(subtree, Decoders[messageNumber].Fields[fieldPrefix .. "A"], buffer, offset,  1)
	subtree:append_text(string.format(": <%d, %d, %d, %d>", r, g, b, a))
	return offset
end

function ExpandZeroCode(buffer, start, count)
	local data = ByteArray.new()
	local srcIndex = start
	local destIndex = 0
	while srcIndex < start + count do
		local b = buffer(srcIndex, 1):uint()
		srcIndex = srcIndex + 1
		if b ~= 0 then
			data:append(ByteArray.new(string.format("%02x", b)))
			destIndex = destIndex + 1
		else
			local repeatCount = 0
			local b = buffer(srcIndex, 1):uint()
			srcIndex = srcIndex + 1
			while b == 0 do
				repeatCount = repeatCount + 256
				local b = buffer(srcIndex, 1):uint()
				srcIndex = srcIndex + 1
			end
			repeatCount = repeatCount + b
			for i = 0, repeatCount - 1, 1 do
				data:append(ByteArray.new(string.format("%02x", 0)))
				destIndex = destIndex + 1
			end
		end
	end
	return data:tvb("Zero expanded")
end

function bitand(a, b)
    local result = 0
    local bitval = 1
    while a > 0 and b > 0 do
      if a % 2 == 1 and b % 2 == 1 then -- test the rightmost bits
          result = result + bitval      -- set the current bit
      end
      bitval = bitval * 2 -- shift left
      a = math.floor(a/2) -- shift right
      b = math.floor(b/2)
    end
    return result
end

function trim(s)
   return (s:gsub("^%s*(.-)%s*$", "%1"))
end

-- Decoders

Decoders =
{
    Header =
	{
		Name = "Header",
	    Fields =
		{
			Flags          = ProtoField.uint8 ("llmsg.Header.Flags",          "Flags",          base.HEX),
			SequenceNumber = ProtoField.uint32("llmsg.Header.SequenceNumber", "SequenceNumber", base.DEC),
			ExtraLength    = ProtoField.uint8 ("llmsg.Header.ExtraLength",    "ExtraLength",    base.DEC),
			Extra          = ProtoField.none  ("llmsg.Header.Extra",          "Extra",          base.HEX)
		},
		
		Decoder = function (buffer, offset, tree, pinfo)
			local name = Decoders.Header.Name
			local o = offset
			Header.Flags          = buffer(o, 1):uint(); o = o + 1
			Header.SequenceNumber = buffer(o, 4):uint(); o = o + 4
			Header.ExtraLength    = buffer(o, 1):uint(); o = o + 1
			Header.Extra = {}
			if (Header.ExtraLength > 0) then
				Header.Extra      = buffer(o, Header.ExtraLength); o = o + Header.ExtraLength
			end
  
			local headerFlagsString = HeaderFlagsToString(Header.Flags)
  
			local headerLength = o - offset
			local headerTree = tree:add(llmsg_protocol, buffer(offset, headerLength), name):append_text(" Seq: " .. Header.SequenceNumber .. " Flags: " .. headerFlagsString)
			
			o = offset
			o = AddFieldToTree(headerTree, Decoders.Header.Fields.Flags,          buffer, o, 1, " (" .. headerFlagsString .. ")")
			o = AddFieldToTree(headerTree, Decoders.Header.Fields.SequenceNumber, buffer, o, 4)
			o = AddFieldToTree(headerTree, Decoders.Header.Fields.ExtraLength,    buffer, o, 1)
			if Header.ExtraLength > 0 then
				o = AddFieldToTree(headerTree, Decoders.Header.Fields.Extra,      buffer, o, Header.ExtraLength)
			end
			pinfo.cols.info:append(string.format(" Seq=%d", Header.SequenceNumber))
			return o;
		end
	},
	
	Body =
	{
		Name = "Body",
		Fields =
		{
			MessageNumber  = ProtoField.uint32("llmsg.Body.MessageNumber",    "MessageNumber",  base.HEX)
		},
		
		Decoder = function (buffer, offset, tree, pinfo)
			local name = Decoders.Body.Name
			local o = offset
			Body.Frequency = "High"			
			Body.MessageNumber = buffer(o, 1):uint(); o = o + 1
			if Body.MessageNumber == 0xff then
				Body.Frequency = "Medium"
				Body.MessageNumber = Body.MessageNumber * 256 + buffer(o, 1):uint(); o = o + 1
				if Body.MessageNumber == 0xffff then
					Body.MessageNumber = Body.MessageNumber * 256 + buffer(o, 1):uint(); o = o + 1
					Body.MessageNumber = Body.MessageNumber * 256 + buffer(o, 1):uint(); o = o + 1
					if Body.MessageNumber < 0xfffffffa then
						Body.Frequency = "Low"
					else
						Body.Frequency = "Fixed"
					end
				end
			end
			
			BodyTree = tree:add(llmsg_protocol, buffer(offset), name)
			AddFieldToTree (BodyTree, Decoders.Body.Fields.MessageNumber, buffer, offset, o - offset)
			return o
		end
	},
	
	Ack = 
	{
		Name = "Ack",
		Fields = 
		{
			Ack    = ProtoField.uint32 ("llmsg.Ack",    "Ack",    base.DEC)
		},
		Decoder = function (buffer, offset, tree, pinfo)
			local name = Decoders.Ack.Name
			local subtree = tree:add(llmsg_protocol, buffer(offset), name)
			local o = offset

			local idString = ""
			local ackCount = buffer(length - 1, 1):uint();
			for i = 0, ackCount - 1, 1 do
				if i > 0 then
					idString = idString .. ", "
				end
				idString = string.format("%s%d", idString, buffer(o, 4):uint())
				o = AddFieldToTree (subtree, Decoders.Ack.Fields.Ack,          buffer, o,  4)
			end
			idString = string.format("[%s]", trim(idString))
			pinfo.cols.info:append(string.format (" Ack=%s", idString))	
			subtree:append_text(string.format(" %s", idString))

			return o
		end
	},

	[0x04] =
	{
		Name = "AgentUpdate",
		Fields =
		{
			AgentId         = ProtoField.guid   ("llmsg.AgentUpdate.AgentId",           "AgentId"),
			SessionId       = ProtoField.guid   ("llmsg.AgentUpdate.SessionId",         "SessionId"),

			BodyRotationX   = ProtoField.float  ("llmsg.AgentUpdate.BodyRotationX",     "BodyRotationX"),
			BodyRotationY   = ProtoField.float  ("llmsg.AgentUpdate.BodyRotationY",     "BodyRotationY"),
			BodyRotationZ   = ProtoField.float  ("llmsg.AgentUpdate.BodyRotationZ",     "BodyRotationZ"),
			BodyRotationW   = ProtoField.float  ("llmsg.AgentUpdate.BodyRotationW",     "BodyRotationW"),

			HeadRotationX   = ProtoField.float  ("llmsg.AgentUpdate.HeadRotationX",     "HeadRotationX"),
			HeadRotationY   = ProtoField.float  ("llmsg.AgentUpdate.HeadRotationY",     "HeadRotationY"),
			HeadRotationZ   = ProtoField.float  ("llmsg.AgentUpdate.HeadRotationZ",     "HeadRotationZ"),
			HeadRotationW   = ProtoField.float  ("llmsg.AgentUpdate.HeadRotationW",     "HeadRotationW"),
			
			State           = ProtoField.uint8  ("llmsg.AgentUpdate.State",             "State"),

			CameraCenterX   = ProtoField.float  ("llmsg.AgentUpdate.CameraCenterX",     "CameraCenterX"),
			CameraCenterY   = ProtoField.float  ("llmsg.AgentUpdate.CameraCenterY",     "CameraCenterY"),
			CameraCenterZ   = ProtoField.float  ("llmsg.AgentUpdate.CameraCenterZ",     "CameraCenterZ"),

			CameraAtAxisX   = ProtoField.float  ("llmsg.AgentUpdate.CameraAtAxisX",     "CameraAtAxisX"),
			CameraAtAxisY   = ProtoField.float  ("llmsg.AgentUpdate.CameraAtAxisY",     "CameraAtAxisY"),
			CameraAtAxisZ   = ProtoField.float  ("llmsg.AgentUpdate.CameraAtAxisZ",     "CameraAtAxisZ"),

			CameraLeftAxisX = ProtoField.float  ("llmsg.AgentUpdate.CameraLeftAxisX",   "CameraLeftAxisX"),
			CameraLeftAxisY = ProtoField.float  ("llmsg.AgentUpdate.CameraLeftAxisY",   "CameraLeftAxisY"),
			CameraLeftAxisZ = ProtoField.float  ("llmsg.AgentUpdate.CameraLeftAxisZ",   "CameraLeftAxisZ"),

			CameraUpAxisX   = ProtoField.float  ("llmsg.AgentUpdate.CameraUpAxisX",     "CameraUpAxisX"),
			CameraUpAxisY   = ProtoField.float  ("llmsg.AgentUpdate.CameraUpAxisY",     "CameraUpAxisY"),
			CameraUpAxisZ   = ProtoField.float  ("llmsg.AgentUpdate.CameraUpAxisZ",     "CameraUpAxisZ"),

			Far             = ProtoField.float  ("llmsg.AgentUpdate.Far",               "Far"),

			ControlFlags    = ProtoField.uint32 ("llmsg.AgentUpdate.ControlFlags",     "ControlFlags", base.HEX),
			Flags           = ProtoField.uint8  ("llmsg.AgentUpdate.Flags",            "Flags",        base.HEX)
		},
		
		Decoder = function (buffer, offset, tree, pinfo)
			local messageNumber = 0x04
			local name = Decoders[messageNumber].Name
			local subtree = tree:add(llmsg_protocol, buffer(offset), name)
			local o = offset
			
			if bitand(Header.Flags, 0x80) ~= 0 then
				buffer = ExpandZeroCode(buffer, offset, length - offset)
				o = 0
			end
			
			o = AddFieldToTree      (subtree, Decoders[messageNumber].Fields.AgentId,       buffer, o, 16)
			o = AddFieldToTree      (subtree, Decoders[messageNumber].Fields.SessionId,     buffer, o, 16)
			
			o = AddQuaternionToTree (subtree, messageNumber, "BodyRotation",                buffer, o)
			o = AddQuaternionToTree (subtree, messageNumber, "HeadRotation",                buffer, o)
			
			local state = buffer(o, 1):uint()
			local stateString = ""
			if state == 0 then stateString = " (walking)" elseif state == 1 then stateString = " (mouselook)" elseif state == 2 then stateText = " (typing)" end
			o = AddFieldToTree_le  (subtree, Decoders[messageNumber].Fields.State,          buffer, o,  1, stateString)
			
			o = AddVector3ToTree   (subtree, messageNumber, "CameraCenter",                 buffer, o)
			o = AddVector3ToTree   (subtree, messageNumber, "CameraAtAxis",                 buffer, o)
			o = AddVector3ToTree   (subtree, messageNumber, "CameraLeftAxis",               buffer, o)
			o = AddVector3ToTree   (subtree, messageNumber, "CameraUpAxis",                 buffer, o)

			o = AddFieldToTree_le  (subtree, Decoders[messageNumber].Fields.Far,            buffer, o,  4)
			
			o = AddFieldToTree_le  (subtree, Decoders[messageNumber].Fields.ControlFlags,   buffer, o,  4)
			o = AddFieldToTree     (subtree, Decoders[messageNumber].Fields.Flags,          buffer, o,  1)
			pinfo.cols.info:append(string.format(" %s", GetMessageIdString(name, messageNumber)))
			BodyTree:append_text(string.format(" %s", GetMessageIdString(name, messageNumber)))
			return o
		end
	},

	[0x05] =
	{
		Name = "AgentAnimation",
		Fields =
		{
			AgentId         = ProtoField.guid   ("llmsg.AgentAnimation.AgentId",       "AgentId"),
			SessionId       = ProtoField.guid   ("llmsg.AgentAnimation.SessionId",     "SessionId"),

			AnimId          = ProtoField.guid   ("llmsg.AgentAnimation.AnimId",        "AnimId"),
			StartAnim       = ProtoField.bool   ("llmsg.AgentAnimation.StartAnim",     "StartAnim"),
			
			TypeData        = ProtoField.none  ("llmsg.AgentAnimation.TypeData",      "TypeData", base.HEX)
		},
		
		Decoder = function (buffer, offset, tree, pinfo)
			local messageNumber = 0x05
			local name = Decoders[messageNumber].Name
			local subtree = tree:add(llmsg_protocol, buffer(offset), name)
			local o = offset

			o = AddFieldToTree      (subtree, Decoders[messageNumber].Fields.AgentId,       buffer, o, 16)
			o = AddFieldToTree      (subtree, Decoders[messageNumber].Fields.SessionId,     buffer, o, 16)

			local nAnims = buffer(o, 1):uint(); o = o + 1
			local animTree = subtree:add(llmsg_protocol, buffer(o), "Animations", string.format("(%d)", nAnims))
			for i = 0, nAnims - 1, 1 do
				o = AddFieldToTree  (animTree, Decoders[messageNumber].Fields.AnimId,       buffer, o, 16)
				o = AddFieldToTree  (animTree, Decoders[messageNumber].Fields.StartAnim,    buffer, o,  1)
			end
			
			local len  = buffer(o, 1):uint(); o = o + 1
			o = AddFieldToTree      (subtree, Decoders[messageNumber].Fields.TypeData,      buffer, o,  len)
			
			pinfo.cols.info:append(string.format(" %s", GetMessageIdString(name, messageNumber)))
			BodyTree:append_text(string.format(" %s", GetMessageIdString(name, messageNumber)))
			return o
		end
	},

	[0x0c] =
	{
		Name = "ObjectUpdate",
		Fields =
		{
			RegionHandle       = ProtoField.uint64 ("llmsg.ObjectUpdate.RegionHandle",       "RegionHandle"),
			TimeDilation       = ProtoField.uint16 ("llmsg.ObjectUpdate.TimeDilation",       "TimeDilation"),
			
			Id                 = ProtoField.uint32 ("llmsg.ObjectUpdate.Id",                 "Id"),
			State              = ProtoField.uint8  ("llmsg.ObjectUpdate.State",              "State"),

			FullId             = ProtoField.guid   ("llmsg.ObjectUpdate.FullId",             "FullId"),
			CRC                = ProtoField.uint32 ("llmsg.ObjectUpdate.CRC",                "CRC"),
			PCode              = ProtoField.uint8  ("llmsg.ObjectUpdate.PCode",              "PCode"),
			Material           = ProtoField.uint8  ("llmsg.ObjectUpdate.Material",           "Material"),
			ClickAction        = ProtoField.uint8  ("llmsg.ObjectUpdate.ClickAction",        "ClickAction"),
			ScaleX             = ProtoField.float  ("llmsg.ObjectUpdate.ScaleX",             "ScaleX"),
			ScaleY             = ProtoField.float  ("llmsg.ObjectUpdate.ScaleY",             "ScaleY"),
			ScaleZ             = ProtoField.float  ("llmsg.ObjectUpdate.ScaleZ",             "ScaleZ"),
			ObjectData         = ProtoField.none   ("llmsg.ObjectUpdate.ObjectData",         "ObjectData", base.HEX),

			ParentId           = ProtoField.uint32 ("llmsg.ObjectUpdate.ParentId",           "ParentId"),
			UpdateFlags        = ProtoField.uint32 ("llmsg.ObjectUpdate.UpdateFlags",        "UpdateFlags", base.HEX),
			
			PathCurve          = ProtoField.uint8  ("llmsg.ObjectUpdate.PathCurve",          "PathCurve"),
			ProfileCurve       = ProtoField.uint8  ("llmsg.ObjectUpdate.ProfileCurve",       "ProfileCurve"),
			PathBegin          = ProtoField.uint16 ("llmsg.ObjectUpdate.PathBegin",          "PathBegin"),
			PathEnd            = ProtoField.uint16 ("llmsg.ObjectUpdate.PathEnd",            "PathEnd"),
			PathScaleX         = ProtoField.uint8  ("llmsg.ObjectUpdate.PathScaleX",         "PathScaleX"),
			PathScaleY         = ProtoField.uint8  ("llmsg.ObjectUpdate.PathScaleY",         "PathScaleY"),
			PathShearX         = ProtoField.uint8  ("llmsg.ObjectUpdate.PathShearX",         "PathShearX"),
			PathShearY         = ProtoField.uint8  ("llmsg.ObjectUpdate.PathShearY",         "PathShearY"),
			PathTwist          = ProtoField.int8   ("llmsg.ObjectUpdate.PathTwist",          "PathTwist"),
			PathTwistBegin     = ProtoField.int8   ("llmsg.ObjectUpdate.PathTwistBegin",     "PathTwistBegin"),
			PathRadiusOffset   = ProtoField.int8   ("llmsg.ObjectUpdate.PathRadiusOffset",   "PathRadiusOffset"),
			PathTaperX         = ProtoField.int8   ("llmsg.ObjectUpdate.PathTaperX",         "PathTaperX"),
			PathTaperY         = ProtoField.int8   ("llmsg.ObjectUpdate.PathTaperY",         "PathTaperY"),
			PathRevolutions    = ProtoField.uint8  ("llmsg.ObjectUpdate.PathRevolutions",    "PathRevolutions"),
			PathSkew           = ProtoField.int8   ("llmsg.ObjectUpdate.PathSkew",           "PathSkew"),
			ProfileBegin       = ProtoField.uint16 ("llmsg.ObjectUpdate.ProfileBegin",       "ProfileBegin"),
			ProfileEnd         = ProtoField.uint16 ("llmsg.ObjectUpdate.ProfileEnd",         "ProfileEnd"),
			ProfileHollow      = ProtoField.uint16 ("llmsg.ObjectUpdate.ProfileHollow",      "ProfileHollow"),

			TextureEntry       = ProtoField.none   ("llmsg.ObjectUpdate.TextureEntry",       "TextureEntry", base.HEX),
			TextureAnim        = ProtoField.none   ("llmsg.ObjectUpdate.TextureAnim",        "TextureAnim",  base.HEX),

			NameValue          = ProtoField.string ("llmsg.ObjectUpdate.NameValue",          "NameValue",    base.UNICODE),
			Data               = ProtoField.none   ("llmsg.ObjectUpdate.Data",               "Data",         base.HEX),
			Text               = ProtoField.string ("llmsg.ObjectUpdate.Text",               "Text",         base.UNICODE),
			TextColorR         = ProtoField.uint8  ("llmsg.ObjectUpdate.TextColorR",         "TextColorR"),
			TextColorG         = ProtoField.uint8  ("llmsg.ObjectUpdate.TextColorG",         "TextColorG"),
			TextColorB         = ProtoField.uint8  ("llmsg.ObjectUpdate.TextColorB",         "TextColorB"),
			TextColorA         = ProtoField.uint8  ("llmsg.ObjectUpdate.TextColorA",         "TextColorA"),
			MediaUrl           = ProtoField.string ("llmsg.ObjectUpdate.MediaUrl",           "MediaUrl",     base.UNICODE),

			PSBlock            = ProtoField.none   ("llmsg.ObjectUpdate.PSBlock",            "PSBlock",      base.HEX),

			ExtraParams        = ProtoField.none   ("llmsg.ObjectUpdate.ExtraParams",        "ExtraParams",  base.HEX),

			Sound              = ProtoField.guid   ("llmsg.ObjectUpdate.Sound",              "Sound"),
			OwnerId            = ProtoField.guid   ("llmsg.ObjectUpdate.OwnerId",            "OwnerId"),
			Gain               = ProtoField.float  ("llmsg.ObjectUpdate.Gain",               "Gain"),
			Flags              = ProtoField.uint8  ("llmsg.ObjectUpdate.Flags",              "Flags",        base.HEX),
			Radius             = ProtoField.float  ("llmsg.ObjectUpdate.Radius",             "Radius"),

			JointType          = ProtoField.uint8  ("llmsg.ObjectUpdate.JointType",          "JointType"),
			JointPivotX        = ProtoField.float  ("llmsg.ObjectUpdate.JointPivotX",        "JointPivotX"),
			JointPivotY        = ProtoField.float  ("llmsg.ObjectUpdate.JointPivotY",        "JointPivotY"),
			JointPivotZ        = ProtoField.float  ("llmsg.ObjectUpdate.JointPivotZ",        "JointPivotZ"),
			JointAxisOrAnchorX = ProtoField.float  ("llmsg.ObjectUpdate.JointAxisOrAnchorX", "JointAxisOrAnchorX"),
			JointAxisOrAnchorY = ProtoField.float  ("llmsg.ObjectUpdate.JointAxisOrAnchorY", "JointAxisOrAnchorY"),
			JointAxisOrAnchorZ = ProtoField.float  ("llmsg.ObjectUpdate.JointAxisOrAnchorZ", "JointAxisOrAnchorZ")
		},
		
		Decoder = function (buffer, offset, tree, pinfo)
			local messageNumber = 0x0c
			local name = Decoders[messageNumber].Name
			local subtree = tree:add(llmsg_protocol, buffer(offset), name)
			local o = offset

			if bitand(Header.Flags, 0x80) ~= 0 then
				buffer = ExpandZeroCode(buffer, offset, length - offset)
				o = 0
			end

			local regionDataTree = subtree:add(llmsg_protocol, buffer(o), "RegionData")
			o = AddFieldToTree_le (regionDataTree, Decoders[messageNumber].Fields.RegionHandle,    buffer, o,  8)
			o = AddFieldToTree_le (regionDataTree, Decoders[messageNumber].Fields.TimeDilation,    buffer, o,  2)
			
			local nObjectData = buffer(o, 1):uint(); o = o + 1
			local objectDatasTree = subtree:add(llmsg_protocol, buffer(o), "ObjectData", string.format(" (%d)", nObjectData))
			for i = 0, nObjectData - 1, 1 do
				local objectDataTree = objectDatasTree:add(llmsg_protocol, buffer(o), "ObjectData")
				o = AddFieldToTree_le (objectDataTree, Decoders[messageNumber].Fields.Id,                buffer, o,  4)
				o = AddFieldToTree    (objectDataTree, Decoders[messageNumber].Fields.State,             buffer, o,  1)

				o = AddFieldToTree    (objectDataTree, Decoders[messageNumber].Fields.FullId,            buffer, o,  16)
				o = AddFieldToTree_le (objectDataTree, Decoders[messageNumber].Fields.CRC,               buffer, o,  4)
				o = AddFieldToTree    (objectDataTree, Decoders[messageNumber].Fields.PCode,             buffer, o,  1)
				o = AddFieldToTree    (objectDataTree, Decoders[messageNumber].Fields.Material,          buffer, o,  1)
				o = AddFieldToTree    (objectDataTree, Decoders[messageNumber].Fields.ClickAction,       buffer, o,  1)
				o = AddVector3ToTree  (objectDataTree, messageNumber, "Scale",                           buffer, o)

				local len = buffer(o, 1):uint(); o = o + 1
				o = AddFieldToTree    (objectDataTree, Decoders[messageNumber].Fields.ObjectData,        buffer, o,  len, string.format(" (%d)", len))

				o = AddFieldToTree_le (objectDataTree, Decoders[messageNumber].Fields.ParentId,          buffer, o,  4)
				o = AddFieldToTree_le (objectDataTree, Decoders[messageNumber].Fields.UpdateFlags,       buffer, o,  4)

				local shapeTree = objectDataTree:add(llmsg_protocol, buffer(o), "Shape")
				o = AddFieldToTree    (shapeTree, Decoders[messageNumber].Fields.PathCurve,              buffer, o,  1)
				o = AddFieldToTree    (shapeTree, Decoders[messageNumber].Fields.ProfileCurve,           buffer, o,  1)
				o = AddFieldToTree_le (shapeTree, Decoders[messageNumber].Fields.PathBegin,              buffer, o,  2)
				o = AddFieldToTree_le (shapeTree, Decoders[messageNumber].Fields.PathEnd,                buffer, o,  2)
				o = AddFieldToTree    (shapeTree, Decoders[messageNumber].Fields.PathScaleX,             buffer, o,  1)
				o = AddFieldToTree    (shapeTree, Decoders[messageNumber].Fields.PathScaleY,             buffer, o,  1)
				o = AddFieldToTree    (shapeTree, Decoders[messageNumber].Fields.PathShearX,             buffer, o,  1)
				o = AddFieldToTree    (shapeTree, Decoders[messageNumber].Fields.PathShearY,             buffer, o,  1)
				o = AddFieldToTree    (shapeTree, Decoders[messageNumber].Fields.PathTwist,              buffer, o,  1)
				o = AddFieldToTree    (shapeTree, Decoders[messageNumber].Fields.PathTwistBegin,         buffer, o,  1)
				o = AddFieldToTree    (shapeTree, Decoders[messageNumber].Fields.PathRadiusOffset,       buffer, o,  1)
				o = AddFieldToTree    (shapeTree, Decoders[messageNumber].Fields.PathTaperX,             buffer, o,  1)
				o = AddFieldToTree    (shapeTree, Decoders[messageNumber].Fields.PathTaperY,             buffer, o,  1)
				o = AddFieldToTree    (shapeTree, Decoders[messageNumber].Fields.PathRevolutions,        buffer, o,  1)
				o = AddFieldToTree    (shapeTree, Decoders[messageNumber].Fields.PathSkew,               buffer, o,  1)
				o = AddFieldToTree_le (shapeTree, Decoders[messageNumber].Fields.ProfileBegin,           buffer, o,  2)
				o = AddFieldToTree_le (shapeTree, Decoders[messageNumber].Fields.ProfileEnd,             buffer, o,  2)
				o = AddFieldToTree_le (shapeTree, Decoders[messageNumber].Fields.ProfileHollow,          buffer, o,  2)
				
				len = buffer(o, 2):le_uint(); o = o + 2 --TODO: Supposed to be big endian!
				o = AddFieldToTree    (objectDataTree, Decoders[messageNumber].Fields.TextureEntry,      buffer, o,  len)
				
				len = buffer(o, 1):uint(); o = o + 1
				o = AddFieldToTree    (objectDataTree, Decoders[messageNumber].Fields.TextureAnim,       buffer, o,  len)
				
				len = buffer(o, 2):le_uint(); o = o + 2 --TODO: Supposed to be big endian!
				o = AddFieldToTree    (objectDataTree, Decoders[messageNumber].Fields.NameValue,         buffer, o,  len)

				len = buffer(o, 2):le_uint(); o = o + 2 --TODO: Supposed to be big endian!
				o = AddFieldToTree    (objectDataTree, Decoders[messageNumber].Fields.Data,              buffer, o,  len)

				len = buffer(o, 1):uint(); o = o + 1
				o = AddFieldToTree    (objectDataTree, Decoders[messageNumber].Fields.Text,              buffer, o,  len)
				
				o = AddColor4UToTree(objectDataTree, messageNumber, "TextColor", buffer, o)

				len = buffer(o, 1):uint(); o = o + 1
				o = AddFieldToTree    (objectDataTree, Decoders[messageNumber].Fields.MediaUrl,         buffer, o,  len)
				
				len = buffer(o, 1):uint(); o = o + 1
				o = AddFieldToTree    (objectDataTree, Decoders[messageNumber].Fields.PSBlock,          buffer, o,  len)

				len = buffer(o, 1):uint(); o = o + 1
				o = AddFieldToTree    (objectDataTree, Decoders[messageNumber].Fields.ExtraParams,      buffer, o,  len)
				
				o = AddFieldToTree    (objectDataTree, Decoders[messageNumber].Fields.Sound,            buffer, o,   16)
				o = AddFieldToTree    (objectDataTree, Decoders[messageNumber].Fields.OwnerId,          buffer, o,   16)
				o = AddFieldToTree_le (objectDataTree, Decoders[messageNumber].Fields.Gain,             buffer, o,    4)
				o = AddFieldToTree    (objectDataTree, Decoders[messageNumber].Fields.Flags,            buffer, o,    1)
				o = AddFieldToTree_le (objectDataTree, Decoders[messageNumber].Fields.Radius,           buffer, o,    4)

				o = AddFieldToTree    (objectDataTree, Decoders[messageNumber].Fields.JointType,        buffer, o,    1)
				o = AddVector3ToTree  (objectDataTree, messageNumber, "JointPivot",                     buffer, o)
				o = AddVector3ToTree  (objectDataTree, messageNumber, "JointAxisOrAnchor",              buffer, o)
				-- xxx
			end
			
			pinfo.cols.info:append(string.format(" %s", GetMessageIdString(name, messageNumber)))
			BodyTree:append_text(string.format(" %s", GetMessageIdString(name, messageNumber)))
			return o
		end
	},

	[0x0e] =
	{
		Name = "ObjectUpdateCached",
		Fields =
		{
			RegionHandle    = ProtoField.uint64 ("llmsg.ObjectUpdateCached.RegionHandle",      "RegionHandle"),
			TimeDilation    = ProtoField.uint16 ("llmsg.ObjectUpdateCached.TimeDilation",      "TimeDilation"),
			ObjectId        = ProtoField.uint32 ("llmsg.ObjectUpdateCached.ObjectId",          "ObjectId"),
			CRC             = ProtoField.uint32 ("llmsg.ObjectUpdateCached.CRC",               "CRC"),
			UpdateFlags     = ProtoField.uint32 ("llmsg.ObjectUpdateCached.UpdateFlags",       "UpdateFlags", base.HEX)
		},
		
		Decoder = function (buffer, offset, tree, pinfo)
			local messageNumber = 0x0e
			local name = Decoders[messageNumber].Name
			local subtree = tree:add(llmsg_protocol, buffer(offset), name)
			local o = offset
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.RegionHandle,    buffer, o,  8)
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.TimeDilation,    buffer, o,  2)
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.ObjectId,        buffer, o,  4)
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.CRC,             buffer, o,  4)
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.UpdateFlags,     buffer, o,  4)
			pinfo.cols.info:append(string.format(" %s", GetMessageIdString(name, messageNumber)))
			BodyTree:append_text(string.format(" %s", GetMessageIdString(name, messageNumber)))
			return o
		end
	},

	[0x0f] =
	{
		Name = "ImprovedTerseObjectUpdate",
		Fields =
		{
			RegionHandle    = ProtoField.uint64 ("llmsg.ImprovedTerseObjectUpdate.RegionHandle",      "RegionHandle"),
			TimeDilation    = ProtoField.uint16 ("llmsg.ImprovedTerseObjectUpdate.TimeDilation",      "TimeDilation"),
			
			Data            = ProtoField.none   ("llmsg.ImprovedTerseObjectUpdate.Data",              "Data"),
			TextureEntry    = ProtoField.none   ("llmsg.ImprovedTerseObjectUpdate.TextureEntry",      "TextureEntry")
		},
		
		Decoder = function (buffer, offset, tree, pinfo)
			local messageNumber = 0x0f
			local name = Decoders[messageNumber].Name
			local subtree = tree:add(llmsg_protocol, buffer(offset), name)
			local o = offset
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.RegionHandle,         buffer, o,  8)
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.TimeDilation,         buffer, o,  2)

			local nObjectData = buffer(o, 1):uint(); o = o + 1
			local objectDatasTree = subtree:add(llmsg_protocol, buffer(o), "ObjectData", string.format(" (%d)", nObjectData))
			for i = 0, nObjectData - 1, 1 do
				local objectDataTree = objectDatasTree:add(llmsg_protocol, buffer(o), "ObjectData")
				local len = buffer(o, 1):uint(); o = o + 1
				o = AddFieldToTree (objectDataTree, Decoders[messageNumber].Fields.Data,         buffer, o,  len, string.format(" (%d)", len))
				len = buffer(o, 2):le_uint(); o = o + 2
				o = AddFieldToTree (objectDataTree, Decoders[messageNumber].Fields.TextureEntry, buffer, o,  len, string.format(" (%d)", len))
			end
			pinfo.cols.info:append(string.format(" %s", GetMessageIdString(name, messageNumber)))
			BodyTree:append_text(string.format(" %s", GetMessageIdString(name, messageNumber)))
			return o
		end
	},

	[0xff06] =
	{
		Name = "CoarseLocationUpdate",
		Fields =
		{
			X        = ProtoField.uint8 ("llmsg.CoarseLocationUpdate.X",        "X"),
			Y        = ProtoField.uint8 ("llmsg.CoarseLocationUpdate.Y",        "Y"),
			Z        = ProtoField.uint8 ("llmsg.CoarseLocationUpdate.Z",        "Z"),
			You      = ProtoField.int16 ("llmsg.CoarseLocationUpdate.You",      "You"),
			Prey     = ProtoField.int16 ("llmsg.CoarseLocationUpdate.Prey",     "Prey"),
			AgentId  = ProtoField.guid  ("llmsg.CoarseLocationUpdate.AgentId",  "AgentId")
		},
		
		Decoder = function (buffer, offset, tree, pinfo)
			local messageNumber = 0xff06
			local name = Decoders[messageNumber].Name
			local subtree = tree:add(llmsg_protocol, buffer(offset), name)	
			local o = offset
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.X,            buffer, o,  1)
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.Y,            buffer, o,  1)
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.Z,            buffer, o,  1)
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.You,          buffer, o,  2)
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.Prey,         buffer, o,  2)
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.AgentId,      buffer, o, 16)
			pinfo.cols.info:append(string.format(" %s", GetMessageIdString(name, messageNumber)))
			BodyTree:append_text(string.format(" %s", GetMessageIdString(name, messageNumber)))
			return o
		end
	},

	[0xff0b] = 
	{
		Name = "ParcelPropertiesRequest",
		Fields = 
		{
			AgentId        = ProtoField.guid  ("llmsg.ParcelPropertiesRequest.AgentId",       "AgentId"),
			SessionId      = ProtoField.guid  ("llmsg.ParcelPropertiesRequest.SessionId",     "SessionId"),
			SequenceId     = ProtoField.int32 ("llmsg.ParcelPropertiesRequest.SequenceId",    "SequenceId",  base.DEC),
			West           = ProtoField.float ("llmsg.ParcelPropertiesRequest.West",          "West"),
			South          = ProtoField.float ("llmsg.ParcelPropertiesRequest.South",         "South"),
			East           = ProtoField.float ("llmsg.ParcelPropertiesRequest.East",          "East"),
			North          = ProtoField.float ("llmsg.ParcelPropertiesRequest.North",         "North"),
			SnapSelection  = ProtoField.bool  ("llmsg.ParcelPropertiesRequest.SnapSelection", "SnapSelection")
		},
		
		Decoder = function (buffer, offset, tree, pinfo)
			local messageNumber = 0xff0b
			local name = Decoders[messageNumber].Name
			local subtree = tree:add(llmsg_protocol, buffer(offset), name)
			local o = offset
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.AgentId,       buffer, o, 16)
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.SessionId,     buffer, o, 16)
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.SequenceId,    buffer, o,  4)
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.West,          buffer, o,  4)
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.South,         buffer, o,  4)
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.East,          buffer, o,  4)
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.North,         buffer, o,  4)
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.SnapSelection, buffer, o,  1)
  
			pinfo.cols.info:append(string.format(" %s", GetMessageIdString(name, messageNumber)))
			BodyTree:append_text(string.format(" %s", GetMessageIdString(name, messageNumber)))
			return o
		end
	},

	[0xff0d] =
	{
		Name = "AttachedSound",
		Fields =
		{
			SoundId   = ProtoField.guid  ("llmsg.AttachedSound.SoundId",   "SoundId"),
			ObjectId  = ProtoField.guid  ("llmsg.AttachedSound.ObjectId",  "ObjectId"),
			OwnerId   = ProtoField.guid  ("llmsg.AttachedSound.OwnerId",   "OwnerId"),
			Gain      = ProtoField.float ("llmsg.AttachedSound.Gain",      "Gain"),
			Flags     = ProtoField.uint8 ("llmsg.AttachedSound.Flags",     "Flags",     base.HEX)
		},
		
		Decoder = function (buffer, offset, tree, pinfo)
			local messageNumber = 0xff0d
			local name = Decoders[messageNumber].Name
			local subtree = tree:add(llmsg_protocol, buffer(offset), name)	
			local o = offset
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.SoundId,      buffer, o, 16)
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.ObjectId,     buffer, o, 16)
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.OwnerId,      buffer, o, 16)
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.Gain,         buffer, o,  4)
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.Flags,        buffer, o,  1)
			pinfo.cols.info:append(string.format(" %s", GetMessageIdString(name, messageNumber)))
			BodyTree:append_text(string.format(" %s", GetMessageIdString(name, messageNumber)))
			return o
		end
	},

	[0xff11] =
	{
		Name = "ViewerEffect",
		Fields = 
		{
			AgentId        = ProtoField.guid   ("llmsg.ViewerEffect.AgentId",        "AgentId"),
			SessionId      = ProtoField.guid   ("llmsg.ViewerEffect.SessionId",      "SessionId"),
			Id             = ProtoField.guid   ("llmsg.ViewerEffect.Id",             "Id"),
			AgentId2       = ProtoField.guid   ("llmsg.ViewerEffect.AgentId2",       "AgentId2"),
			Type           = ProtoField.uint8  ("llmsg.ViewerEffect.Type",           "Type",           base.DEC),
			Duration       = ProtoField.float  ("llmsg.ViewerEffect.Duration",       "Duration"),
			Color          = ProtoField.uint32 ("llmsg.ViewerEffect.Color",          "Color",          base.HEX),
			TypeDataLength = ProtoField.uint8  ("llmsg.ViewerEffect.TypeDataLength", "TypeDataLength", base.DEC),
			TypeData       = ProtoField.none   ("llmsg.ViewerEffect.TypeData",       "TypeData",       base.HEX)
		},
		
		Decoder = function (buffer, offset, tree, pinfo)
			local messageNumber = 0xff11
			local name = Decoders[messageNumber].Name
			local subtree = tree:add(llmsg_protocol, buffer(offset), name)
			local o = offset

			if bitand(Header.Flags, 0x80) ~= 0 then
				buffer = ExpandZeroCode(buffer, offset, length - offset)
				o = 0
			end

			o = AddFieldToTree(subtree, Decoders[messageNumber].Fields.AgentId,       buffer, o, 16)
			o = AddFieldToTree(subtree, Decoders[messageNumber].Fields.SessionId,     buffer, o, 16)
	
			local nEffects = buffer(o, 1):uint(); o = o + 1
			local effectsTree = subtree:add(llmsg_protocol, buffer(o), "Effects", string.format("(%d)", nEffects))
			for i = 0, nEffects - 1, 1 do
				local effectTree = effectsTree:add(llmsg_protocol, buffer(o), "Effect")
				o = AddFieldToTree    (effectTree, Decoders[messageNumber].Fields.Id,             buffer, o, 16)
				o = AddFieldToTree    (effectTree, Decoders[messageNumber].Fields.AgentId2,       buffer, o, 16)
				o = AddFieldToTree    (effectTree, Decoders[messageNumber].Fields.Type,           buffer, o,  1)
				o = AddFieldToTree_le (effectTree, Decoders[messageNumber].Fields.Duration,       buffer, o,  4)
				o = AddFieldToTree_le (effectTree, Decoders[messageNumber].Fields.Color,          buffer, o,  4)
				local typeDataLength = buffer(o, 1):uint();
				o = AddFieldToTree    (effectTree, Decoders[messageNumber].Fields.TypeDataLength, buffer, o,  1)
				o = AddFieldToTree    (effectTree, Decoders[messageNumber].Fields.TypeData,       buffer, o,  typeDataLength)
			end
			pinfo.cols.info:append(string.format(" %s", GetMessageIdString(name, messageNumber)))
			BodyTree:append_text(string.format(" %s", GetMessageIdString(name, messageNumber)))
			return o
		end
	},
	
	[0xffff0001] =
	{
		Name = "Wrapper",
		Fields =
		{
			Id = ProtoField.uint8  ("llmsg.Wrapper.Id", "Id", base.DEC)
		},
		
		Decoder = function (buffer, offset, tree, pinfo)
			local messageNumber = 0xffff0001
			local name = Decoders[messageNumber].Name
			local subtree = tree:add(llmsg_protocol, buffer(offset), name)
			local o = offset
			local id = buffer(o, 1):uint()
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.Id,            buffer, o,   1)

			pinfo.cols.info:append(string.format(" %s: ", GetMessageIdString(name, messageNumber)))
			BodyTree:append_text(string.format(" %s: ", GetMessageIdString(name, messageNumber)))
			
			id = id + 0xffff0000

			local unknown = false
			if Decoders[id] ~= nil then
				o = Decoders[id].Decoder(buffer, o, tree, pinfo)
			else
				pinfo.cols.info:append(string.format(" %s", GetMessageIdString("Unknown", id)))
				BodyTree:append_text(string.format(" %s", GetMessageIdString("Unknown", id)))
				unknown = true
			end
			
			return o
		end
	},
	
	[0xffff0003] = 
	{
		Name = "UseCircuitCode",
		Fields = 
		{
			Code           = ProtoField.uint32("llmsg.UseCircuitCode.Code",      "Code",        base.HEX),
			SessionId      = ProtoField.guid  ("llmsg.UseCircuitCode.SessionId", "SessionId"),
			Id             = ProtoField.guid  ("llmsg.UseCircuitCode.Id",        "Id")
		},
		Decoder = function (buffer, offset, tree, pinfo)
			local messageNumber = 0xffff0003
			local name = Decoders[messageNumber].Name
			local subtree = tree:add(llmsg_protocol, buffer(offset), name)
			local o = offset
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.Code,      buffer, o,  4)
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.SessionId, buffer, o, 16)
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.Id,        buffer, o, 16, " (AgentId)")
  
			pinfo.cols.info:append(string.format(" %s", GetMessageIdString(name, messageNumber)))
			BodyTree:append_text(string.format(" %s", GetMessageIdString(name, messageNumber)))
			return o
		end
	},
	
	[0xffff0050] =
	{
		Name = "ChatFromViewer",
		Fields =
		{
			AgentId         = ProtoField.guid   ("llmsg.ChatFromViewer.AgentId",           "AgentId"),
			SessionId       = ProtoField.guid   ("llmsg.ChatFromViewer.SessionId",         "SessionId"),
			Message         = ProtoField.string ("llmsg.ChatFromViewer.Message",           "Message",   base.UNICODE),
			Type            = ProtoField.uint8  ("llmsg.ChatFromViewer.Type",              "Type"),
			Channel         = ProtoField.uint8  ("llmsg.ChatFromViewer.Channel",           "Channel")
		},
		
		Decoder = function (buffer, offset, tree, pinfo)
			local messageNumber = 0xffff0050
			local name = Decoders[messageNumber].Name
			local subtree = tree:add(llmsg_protocol, buffer(offset), name)
			local o = offset
			
			if bitand(Header.Flags, 0x80) ~= 0 then
				buffer = ExpandZeroCode(buffer, offset, length - offset)
				o = 0
			end
			
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.AgentId,       buffer, o,   16)
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.SessionId,     buffer, o,   16)
			local len = buffer(o, 2):le_uint(); o = o + 2
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.Message,       buffer, o,  len)
			
			local type = buffer(o, 1):uint()
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.Type,          buffer, o,    1, string.format(" (%s)", ChatTypeToString(type)))
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.Channel,       buffer, o,    4)
			
			pinfo.cols.info:append(string.format(" %s", GetMessageIdString(name, messageNumber)))
			BodyTree:append_text(string.format(" %s", GetMessageIdString(name, messageNumber)))
			return o
		end
	},

	[0xffff0051] =
	{
		Name = "AgentThrottle",
		Fields =
		{
			AgentId        = ProtoField.guid   ("llmsg.AgentThrottle.AgentId",        "AgentId"),
			SessionId      = ProtoField.guid   ("llmsg.AgentThrottle.SessionId",      "SessionId"),
			CircuitCode    = ProtoField.uint32 ("llmsg.AgentThrottle.CircuitCode",    "CircuitCode",       base.HEX),
			GenCounter     = ProtoField.uint32 ("llmsg.AgentThrottle.GenCounter",     "GenCounter",        base.HEX),
			Throttle       = ProtoField.uint32 ("llmsg.AgentThrottle.Throttle",       "Throttle",          base.DEC)
		},
		
		Decoder = function (buffer, offset, tree, pinfo)
			local messageNumber = 0xffff0051
			local name = Decoders[messageNumber].Name
			local subtree = tree:add(llmsg_protocol, buffer(offset), name)
			local o = offset
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.AgentId,      buffer, o, 16)
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.SessionId,    buffer, o, 16)
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.CircuitCode,  buffer, o,  4)
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.GenCounter,   buffer, o,  4)
			
			local nThrottles = buffer(o, 1):uint(); o = o + 1
			local throttleTree = subtree:add(llmsg_protocol, buffer(offset), "Throttles", string.format("(%d)", nThrottles))
			for i = 0, nThrottles - 1, 1 do
				o = AddFieldToTree (throttleTree, Decoders[messageNumber].Fields.Throttle, buffer, o,  1)
			end
			pinfo.cols.info:append(string.format(" %s", GetMessageIdString(name, messageNumber)))
			BodyTree:append_text(string.format(" %s", GetMessageIdString(name, messageNumber)))
			return o
		end
	},

	[0xffff0053] =
	{
		Name = "AgentHeightWidth",
		Fields =
		{
			AgentId        = ProtoField.guid   ("llmsg.AgentHeightWidth.AgentId",        "AgentId"),
			SessionId      = ProtoField.guid   ("llmsg.AgentHeightWidth.SessionId",      "SessionId"),
			CircuitCode    = ProtoField.uint32 ("llmsg.AgentHeightWidth.CircuitCode",    "CircuitCode",       base.HEX),
			GenCounter     = ProtoField.uint32 ("llmsg.AgentHeightWidth.GenCounter",     "GenCounter",        base.HEX),
			Height         = ProtoField.uint16 ("llmsg.AgentHeightWidth.Height",         "Height",            base.DEC),
			Width          = ProtoField.uint16 ("llmsg.AgentHeightWidth.Width",          "Width",             base.DEC)
		},
		
		Decoder = function (buffer, offset, tree, pinfo)
			local messageNumber = 0xffff0053
			local name = Decoders[messageNumber].Name
			local subtree = tree:add(llmsg_protocol, buffer(offset), name)
			local o = offset
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.AgentId,      buffer, o, 16)
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.SessionId,    buffer, o, 16)
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.CircuitCode,  buffer, o,  4)
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.GenCounter,   buffer, o,  4)
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.Height,       buffer, o,  2)
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.Width,        buffer, o,  2)
			pinfo.cols.info:append(string.format(" %s", GetMessageIdString(name, messageNumber)))
			BodyTree:append_text(string.format(" %s", GetMessageIdString(name, messageNumber)))
			return o
		end
	},

	[0xffff0058] =
	{
		Name = "SetAlwaysRun",
		Fields =
		{
			AgentId        = ProtoField.guid ("llmsg.SetAlwaysRun.AgentId",        "AgentId"),
			SessionId      = ProtoField.guid ("llmsg.SetAlwaysRun.SessionId",      "SessionId"),
			AlwaysRun      = ProtoField.bool ("llmsg.SetAlwaysRun.AlwaysRun",      "AlwaysRun")
		},
		
		Decoder = function (buffer, offset, tree, pinfo)
			local messageNumber = 0xffff0058
			local name = Decoders[messageNumber].Name
			local subtree = tree:add(llmsg_protocol, buffer(offset), name)
			local o = offset
			o = AddFieldToTree (subtree, Decoders[messageNumber].Fields.AgentId,      buffer, o, 16)
			o = AddFieldToTree (subtree, Decoders[messageNumber].Fields.SessionId,    buffer, o, 16)
			o = AddFieldToTree (subtree, Decoders[messageNumber].Fields.AlwaysRun,    buffer, o,  1)
			pinfo.cols.info:append(string.format(" %s", GetMessageIdString(name, messageNumber)))
			BodyTree:append_text(string.format(" %s", GetMessageIdString(name, messageNumber)))
			return o
		end
	},

	[0xffff008a] =
	{
		Name = "HealthMessage",
		Fields =
		{
			Health       = ProtoField.float  ("llmsg.HealthMessage.Health",       "Health"),
		},
		
		Decoder = function (buffer, offset, tree, pinfo)
			local messageNumber = 0xffff008a
			local name = Decoders[messageNumber].Name
			local subtree = tree:add(llmsg_protocol, buffer(offset), name)
			local o = offset
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.Health,       buffer, o,  4)
			pinfo.cols.info:append(string.format(" %s", GetMessageIdString(name, messageNumber)))
			BodyTree:append_text(string.format(" %s", GetMessageIdString(name, messageNumber)))
			return o
		end
	},

	[0xffff0094] =
	{
		Name = "RegionHandshake",
		Fields = 
		{
			RegionFlags          = ProtoField.uint32 ("llmsg.RegionHandshake.RegionFlags",            "RegionFlags",        base.HEX),
			SimAccess            = ProtoField.uint8  ("llmsg.RegionHandshake.SimAccess",              "SimAccess"),
			SimName              = ProtoField.string ("llmsg.RegionHandshake.SimName",                "SimName"),
			SimOwner             = ProtoField.guid   ("llmsg.RegionHandshake.SimOwner",               "SimOwner"),
			IsEstateManager      = ProtoField.bool   ("llmsg.RegionHandshake.IsEstateManager",        "IsEstateManager"),
			WaterHeight          = ProtoField.float  ("llmsg.RegionHandshake.WaterHeight",            "WaterHeight"),
			BillableFactor       = ProtoField.float  ("llmsg.RegionHandshake.BillableFactor",         "BillableFactor"),
			CacheId              = ProtoField.guid   ("llmsg.RegionHandshake.CacheId",                "CacheId"),
			TerrainBase0         = ProtoField.guid   ("llmsg.RegionHandshake.TerrainBase0",           "TerrainBase0"),
			TerrainBase1         = ProtoField.guid   ("llmsg.RegionHandshake.TerrainBase1",           "TerrainBase1"),
			TerrainBase2         = ProtoField.guid   ("llmsg.RegionHandshake.TerrainBase2",           "TerrainBase2"),
			TerrainBase3         = ProtoField.guid   ("llmsg.RegionHandshake.TerrainBase3",           "TerrainBase3"),
			TerrainDetail0       = ProtoField.guid   ("llmsg.RegionHandshake.TerrainDetail0",         "TerrainDetail0"),
			TerrainDetail1       = ProtoField.guid   ("llmsg.RegionHandshake.TerrainDetail1",         "TerrainDetail1"),
			TerrainDetail2       = ProtoField.guid   ("llmsg.RegionHandshake.TerrainDetail2",         "TerrainDetail2"),
			TerrainDetail3       = ProtoField.guid   ("llmsg.RegionHandshake.TerrainDetail3",         "TerrainDetail3"),
			TerrainStartHeight00 = ProtoField.float  ("llmsg.RegionHandshake.TerrainStartHeight00",   "TerrainStartHeight00"),
			TerrainStartHeight01 = ProtoField.float  ("llmsg.RegionHandshake.TerrainStartHeight01",   "TerrainStartHeight01"),
			TerrainStartHeight10 = ProtoField.float  ("llmsg.RegionHandshake.TerrainStartHeight10",   "TerrainStartHeight10"),
			TerrainStartHeight11 = ProtoField.float  ("llmsg.RegionHandshake.TerrainStartHeight11",   "TerrainStartHeight11"),
			TerrainHeightRange00 = ProtoField.float  ("llmsg.RegionHandshake.TerrainHeightRange00",   "TerrainHeightRange00"),
			TerrainHeightRange01 = ProtoField.float  ("llmsg.RegionHandshake.TerrainHeightRange01",   "TerrainHeightRange01"),
			TerrainHeightRange10 = ProtoField.float  ("llmsg.RegionHandshake.TerrainHeightRange10",   "TerrainHeightRange10"),
			TerrainHeightRange11 = ProtoField.float  ("llmsg.RegionHandshake.TerrainHeightRange11",   "TerrainHeightRange11"),

			RegionId             = ProtoField.guid   ("llmsg.RegionHandshake.RegionId",               "RegionId"),

			CPUClassId           = ProtoField.int32  ("llmsg.RegionHandshake.CPUClassId",             "CPUClassId"),
			CPURatio             = ProtoField.int32  ("llmsg.RegionHandshake.CPURatio",               "CPURatio"),
			ColoName             = ProtoField.string ("llmsg.RegionHandshake.ColoName",               "ColoName"),
			ProductSKU           = ProtoField.string ("llmsg.RegionHandshake.ProductSKU",             "ProductSKU"),
			ProductName          = ProtoField.string ("llmsg.RegionHandshake.ProductName",            "ProductName"),
			
			RegionFlagsExtended  = ProtoField.uint64 ("llmsg.RegionHandshake.RegionFlagsExtended",    "RegionFlagsExtended", base.HEX),
			RegionProtocols      = ProtoField.uint64 ("llmsg.RegionHandshake.RegionProtocols",        "RegionProtocols",     base.HEX)

		},
		Decoder = function (buffer, offset, tree, pinfo)
			local messageNumber = 0xffff0094
			local name = Decoders[messageNumber].Name
			local subtree = tree:add(llmsg_protocol, buffer(offset), name)
			local o = offset
			
			if bitand(Header.Flags, 0x80) ~= 0 then
				buffer = ExpandZeroCode(buffer, offset, length - offset)
				o = 0
			end
			
			local regionFlags = buffer(o, 4):uint()
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.RegionFlags,          buffer, o,   4, string.format(" (%s)", RegionFlagsToString(regionFlags)))
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.SimAccess,            buffer, o,   1)
			local len = buffer(o, 1):uint(); o = o + 1
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.SimName,              buffer, o, len)
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.SimOwner,             buffer, o,  16)
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.IsEstateManager,      buffer, o,   1)
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.WaterHeight,          buffer, o,   4)
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.BillableFactor,       buffer, o,   4)
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.CacheId,              buffer, o,  16)
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.TerrainBase0,         buffer, o,  16)
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.TerrainBase1,         buffer, o,  16)
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.TerrainBase2,         buffer, o,  16)
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.TerrainBase3,         buffer, o,  16)
  			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.TerrainDetail0,       buffer, o,  16)
  			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.TerrainDetail1,       buffer, o,  16)
  			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.TerrainDetail2,       buffer, o,  16)
  			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.TerrainDetail3,       buffer, o,  16)
  			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.TerrainStartHeight00, buffer, o,   4)
  			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.TerrainStartHeight01, buffer, o,   4)
  			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.TerrainStartHeight10, buffer, o,   4)
  			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.TerrainStartHeight11, buffer, o,   4)
  			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.TerrainHeightRange00, buffer, o,   4)
  			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.TerrainHeightRange01, buffer, o,   4)
  			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.TerrainHeightRange10, buffer, o,   4)
  			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.TerrainHeightRange11, buffer, o,   4)

			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.RegionId,             buffer, o,  16)

  			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.CPUClassId,           buffer, o,   4)
  			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.CPURatio,             buffer, o,   4)
			len = buffer(o, 1):uint(); o = o + 1
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.ColoName,             buffer, o, len)
			len = buffer(o, 1):uint(); o = o + 1
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.ProductSKU,           buffer, o, len)
			len = buffer(o, 1):uint(); o = o + 1
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.ProductName,          buffer, o, len)

			len = buffer(o, 1):uint(); o = o + 1
			local regionInfo4Tree = subtree:add(llmsg_protocol, buffer(offset), "RegionInfo4", string.format("(%d)", len))
			for i = 0, len - 1, 1 do
				local regionInfoTree = regionInfo4Tree:add(llmsg_protocol, buffer(offset), "RegionInfo")
				o = AddFieldToTree    (regionInfoTree, Decoders[messageNumber].Fields.RegionFlagsExtended, buffer, o, 8)
				o = AddFieldToTree    (regionInfoTree, Decoders[messageNumber].Fields.RegionProtocols,     buffer, o, 8)
			end
			pinfo.cols.info:append(string.format(" %s", GetMessageIdString(name, messageNumber)))
			BodyTree:append_text(string.format(" %s", GetMessageIdString(name, messageNumber)))
			return o
		end
	},

	[0xffff0095] =
	{
		Name = "RegionHandshakeReply",
		Fields =
		{
			AgentId        = ProtoField.guid   ("llmsg.RegionHandshakeReply.AgentId",        "AgentId"),
			SessionId      = ProtoField.guid   ("llmsg.RegionHandshakeReply.SessionId",      "SessionId"),
			Flags          = ProtoField.uint32 ("llmsg.RegionHandshakeReply.Flags",          "Flags",       base.HEX)
		},
		
		Decoder = function (buffer, offset, tree, pinfo)
			local messageNumber = 0xffff0095
			local name = Decoders[messageNumber].Name
			local subtree = tree:add(llmsg_protocol, buffer(offset), name)
			local o = offset
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.AgentId,      buffer, o, 16)
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.SessionId,    buffer, o, 16)
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.Flags,        buffer, o,  4)
			pinfo.cols.info:append(string.format(" %s", GetMessageIdString(name, messageNumber)))
			BodyTree:append_text(string.format(" %s", GetMessageIdString(name, messageNumber)))
			return o
		end
	},

	[0xffff00c4] =
	{
		Name = "ParcelOverlay",
		Fields =
		{
			SequenceId     = ProtoField.uint32 ("llmsg.ParcelOverlay.SequenceId",    "SequenceId",   base.HEX),
			Data           = ProtoField.none   ("llmsg.ParcelOverlay.Data",          "Data",         base.HEX)
		},
		
		Decoder = function (buffer, offset, tree, pinfo)
			local messageNumber = 0xffff00c4
			local name = Decoders[messageNumber].Name
			local subtree = tree:add(llmsg_protocol, buffer(offset), name)
			local o = offset

			if bitand(Header.Flags, 0x80) ~= 0 then
				buffer = ExpandZeroCode(buffer, offset, length - offset)
				o = 0
			end

			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.SequenceId,        buffer, o,  4)
			local nSections = buffer(o, 2):uint(); o = o + 2
			local sectionsTree = subtree:add(llmsg_protocol, buffer(offset), "Sections", string.format("(%d)", nSections))
			for i = 0, nSections - 1, 1 do
				o = AddFieldToTree    (sectionsTree, Decoders[messageNumber].Fields.Data,     buffer, o,  0x100, " (256 parcel units, 8 bits per parcel unit)")
			end
			
			pinfo.cols.info:append(string.format(" %s", GetMessageIdString(name, messageNumber)))
			BodyTree:append_text(string.format(" %s", GetMessageIdString(name, messageNumber)))
			return o
		end
	},

	[0xffff00f9] =
	{
		Name = "CompleteAgentMovement",
		Fields =
		{
			AgentId        = ProtoField.guid   ("llmsg.CompleteAgentMovement.AgentId",        "AgentId"),
			SessionId      = ProtoField.guid   ("llmsg.CompleteAgentMovement.SessionId",      "SessionId"),
			CircuitCode    = ProtoField.uint32("llmsg.CompleteAgentMovement.CircuitCode",     "CircuitCode",        base.HEX)
		},
		
		Decoder = function (buffer, offset, tree, pinfo)
			local messageNumber = 0xffff00f9
			local name = Decoders[messageNumber].Name
			local subtree = tree:add(llmsg_protocol, buffer(offset), name)
			local o = offset
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.AgentId,      buffer, o, 16)
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.SessionId,    buffer, o, 16)
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.CircuitCode,  buffer, o,  4)
  
			pinfo.cols.info:append(string.format(" %s", GetMessageIdString(name, messageNumber)))
			BodyTree:append_text(string.format(" %s", GetMessageIdString(name, messageNumber)))
			return o
		end
	},

	[0xffff00fa] =
	{
		Name = "AgentMovementComplete",
		Fields =
		{
			AgentId        = ProtoField.guid          ("llmsg.CompleteAgentMovement.AgentId",       "AgentId"),
			SessionId      = ProtoField.guid          ("llmsg.CompleteAgentMovement.SessionId",     "SessionId"),

			PositionX      = ProtoField.float         ("llmsg.CompleteAgentMovement.PositionX",     "PositionX"),
			PositionY      = ProtoField.float         ("llmsg.CompleteAgentMovement.PositionY",     "PositionY"),
			PositionZ      = ProtoField.float         ("llmsg.CompleteAgentMovement.PositionZ",     "PositionZ"),

			LookAtX        = ProtoField.float         ("llmsg.CompleteAgentMovement.LookAtX",       "LookAtX"),
			LookAtY        = ProtoField.float         ("llmsg.CompleteAgentMovement.LookAtY",       "LookAtY"),
			LookAtZ        = ProtoField.float         ("llmsg.CompleteAgentMovement.LookAtZ",       "LookAtZ"),

			RegionHandle   = ProtoField.uint64        ("llmsg.CompleteAgentMovement.RegionHandle",  "RegionHandle",     base.HEX),
			Timestamp      = ProtoField.absolute_time ("llmsg.CompleteAgentMovement.Timestamp",     "Timestamp"),
		
			ChannelVersion = ProtoField.string("llmsg.CompleteAgentMovement.ChannelVersion",        "ChannelVersion",   base.UNICODE)
		},
		
		Decoder = function (buffer, offset, tree, pinfo)
			local messageNumber = 0xffff00fa
			local name = Decoders[messageNumber].Name
			local subtree = tree:add(llmsg_protocol, buffer(offset), name)
			local o = offset
			o = AddFieldToTree(subtree, Decoders[messageNumber].Fields.AgentId,             buffer, o, 16)
			o = AddFieldToTree(subtree, Decoders[messageNumber].Fields.SessionId,           buffer, o, 16)

			o = AddVector3ToTree(subtree, messageNumber, "Position",                        buffer, o)
			o = AddVector3ToTree(subtree, messageNumber, "LookAt",                          buffer, o)

			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.RegionHandle,    buffer, o,  8)
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.Timestamp,       buffer, o,  4)

			local len = buffer(o, 2):le_uint(); o = o + 2
			o = AddFieldToTree(subtree, Decoders[messageNumber].Fields.ChannelVersion, buffer, o,  len)

			pinfo.cols.info:append(string.format(" %s", GetMessageIdString(name, messageNumber)))
			BodyTree:append_text(string.format(" %s", GetMessageIdString(name, messageNumber)))
			return o
		end
	},

	[0xffff0106] =
	{
		Name = "MuteListRequest",
		Fields =
		{
			AgentId        = ProtoField.guid   ("llmsg.MuteListRequest.AgentId",        "AgentId"),
			SessionId      = ProtoField.guid   ("llmsg.MuteListRequest.SessionId",      "SessionId"),
			MuteCRC        = ProtoField.uint32 ("llmsg.MuteListRequest.MuteCRC",        "MuteCRC")
		},
		
		Decoder = function (buffer, offset, tree, pinfo)
			local messageNumber = 0xffff0106
			local name = Decoders[messageNumber].Name
			local subtree = tree:add(llmsg_protocol, buffer(offset), name)
			local o = offset
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.AgentId,      buffer, o, 16)
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.SessionId,    buffer, o, 16)
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.MuteCRC,      buffer, o,  4)
			pinfo.cols.info:append(string.format(" %s", GetMessageIdString(name, messageNumber)))
			BodyTree:append_text(string.format(" %s", GetMessageIdString(name, messageNumber)))
			return o
		end
	},

	[0xffff0139] =
	{
		Name = "MoneyBalanceRequest",
		Fields =
		{
			AgentId        = ProtoField.guid ("llmsg.MoneyBalanceRequest.AgentId",        "AgentId"),
			SessionId      = ProtoField.guid ("llmsg.MoneyBalanceRequest.SessionId",      "SessionId"),
			TransactionID  = ProtoField.guid ("llmsg.MoneyBalanceRequest.TransactionID",  "TransactionID")
		},
		
		Decoder = function (buffer, offset, tree, pinfo)
			local messageNumber = 0xffff0139
			local name = Decoders[messageNumber].Name
			local subtree = tree:add(llmsg_protocol, buffer(offset), name)
			local o = offset
			
			if bitand(Header.Flags, 0x80) ~= 0 then
				buffer = ExpandZeroCode(buffer, o, length - o)
				o = 0
			end
			
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.AgentId,       buffer, o, 16)
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.SessionId,     buffer, o, 16)
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.TransactionID, buffer, o, 16)
			pinfo.cols.info:append(string.format(" %s", GetMessageIdString(name, messageNumber)))
			BodyTree:append_text(string.format(" %s", GetMessageIdString(name, messageNumber)))
			return o
		end
	},

	[0xffff013a] =
	{
		Name = "MoneyBalanceReply",
		Fields =
		{
			AgentId               = ProtoField.guid   ("llmsg.MoneyBalanceReply.AgentId",               "AgentId"),
			TransactionID         = ProtoField.guid   ("llmsg.MoneyBalanceReply.TransactionID",         "TransactionID"),
			TransactionSuccess    = ProtoField.bool   ("llmsg.MoneyBalanceReply.TransactionSuccess",    "TransactionSuccess"),
			MoneyBalance          = ProtoField.int32  ("llmsg.MoneyBalanceReply.MoneyBalance",          "MoneyBalance"),
			SquareMetersCredit    = ProtoField.int32  ("llmsg.MoneyBalanceReply.SquareMetersCredit",    "SquareMetersCredit"),
			SquareMetersCommitted = ProtoField.int32  ("llmsg.MoneyBalanceReply.SquareMetersCommitted", "SquareMetersCommitted"),
			Description           = ProtoField.string ("llmsg.MoneyBalanceReply.Description",           "Description", base.UNICODE),

			TransactionType       = ProtoField.int32  ("llmsg.MoneyBalanceReply.TransactionType",       "TransactionType"),
			SourceId              = ProtoField.guid   ("llmsg.MoneyBalanceReply.SourceId",              "SourceId"),
			IsSourceGroup         = ProtoField.bool   ("llmsg.MoneyBalanceReply.IsSourceGroup",         "IsSourceGroup"),
			DestId                = ProtoField.guid   ("llmsg.MoneyBalanceReply.DestId",                "DestId"),
			IsDestGroup           = ProtoField.bool   ("llmsg.MoneyBalanceReply.IsDestGroup",           "IsDestGroup"),
			Amount                = ProtoField.int32  ("llmsg.MoneyBalanceReply.Amount",                "Amount"),
			ItemDescription       = ProtoField.string ("llmsg.MoneyBalanceReply.ItemDescription",       "ItemDescription", base.UNICODE)
		},
		
		Decoder = function (buffer, offset, tree, pinfo)
			local messageNumber = 0xffff013a
			local name = Decoders[messageNumber].Name
			local subtree = tree:add(llmsg_protocol, buffer(offset), name)
			local o = offset
			
			if bitand(Header.Flags, 0x80) ~= 0 then
				buffer = ExpandZeroCode(buffer, o, length - o)
				o = 0
			end
			
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.AgentId,               buffer, o, 16)
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.TransactionID,         buffer, o, 16)
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.TransactionSuccess,    buffer, o,  1)
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.MoneyBalance,          buffer, o,  4)
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.SquareMetersCredit,    buffer, o,  4)
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.SquareMetersCommitted, buffer, o,  4)
			local len = buffer(o, 1):uint(); o = o + 1
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.Description,           buffer, o,  len)

			local transactionType = buffer(o, 4):int()
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.TransactionType,       buffer, o,  4, string.format(" (%s)", TransactionTypeToString(transactionType)))
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.SourceId,              buffer, o, 16)
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.IsSourceGroup,         buffer, o,  1)
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.DestId,                buffer, o, 16)
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.IsDestGroup,           buffer, o,  1)
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.Amount,                buffer, o,  4)
			len = buffer(o, 1):uint(); o = o + 1
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.ItemDescription,       buffer, o,  len)

			pinfo.cols.info:append(string.format(" %s", GetMessageIdString(name, messageNumber)))
			BodyTree:append_text(string.format(" %s", GetMessageIdString(name, messageNumber)))
			return o
		end
	},

	[0xffff013e] =
	{
		Name = "MuteListUpdate",
		Fields =
		{
			AgentId        = ProtoField.guid          ("llmsg.MuteListUpdate.AgentId",       "AgentId"),
			Filename       = ProtoField.string        ("llmsg.MuteListUpdate.Filename",      "Filename", base.UNICODE)
		},
		
		Decoder = function (buffer, offset, tree, pinfo)
			local messageNumber = 0xffff013e
			local name = Decoders[messageNumber].Name
			local subtree = tree:add(llmsg_protocol, buffer(offset), name)
			local o = offset
			o = AddFieldToTree(subtree, Decoders[messageNumber].Fields.AgentId,             buffer, o, 16)
			local len = buffer(o, 1):uint(); o = o + 1
			o = AddFieldToTree(subtree, Decoders[messageNumber].Fields.Filename,            buffer, o, len)
			pinfo.cols.info:append(string.format(" %s", GetMessageIdString(name, messageNumber)))
			BodyTree:append_text(string.format(" %s", GetMessageIdString(name, messageNumber)))
			return o
		end
	},

	[0xffff0142] =
	{
		Name = "OnlineNotification",
		Fields =
		{
			AgentId        = ProtoField.guid          ("llmsg.OnlineNotification.AgentId",       "AgentId"),
		},
		
		Decoder = function (buffer, offset, tree, pinfo)
			local messageNumber = 0xffff0142
			local name = Decoders[messageNumber].Name
			local subtree = tree:add(llmsg_protocol, buffer(offset), name)
			local o = offset
			local agentIdString = GuidToString(buffer(o, 16))
			o = AddFieldToTree(subtree, Decoders[messageNumber].Fields.AgentId,             buffer, o, 16)
			pinfo.cols.info:append(string.format(" %s (%s)", GetMessageIdString(name, messageNumber), agentIdString))
			BodyTree:append_text(string.format(" %s", GetMessageIdString(name, messageNumber)))
			return o
		end
	},

	[0xffff0143] =
	{
		Name = "OfflineNotification",
		Fields =
		{
			AgentId        = ProtoField.guid          ("llmsg.OfflineNotification.AgentId",       "AgentId"),
		},
		
		Decoder = function (buffer, offset, tree, pinfo)
			local messageNumber = 0xffff0143
			local name = Decoders[messageNumber].Name
			local subtree = tree:add(llmsg_protocol, buffer(offset), name)
			local o = offset
			local agentIdString = GuidToString(buffer(o, 16))
			o = AddFieldToTree(subtree, Decoders[messageNumber].Fields.AgentId,             buffer, o, 16)
			pinfo.cols.info:append(string.format(" %s (%s)", GetMessageIdString(name, messageNumber), agentIdString))
			BodyTree:append_text(string.format(" %s", GetMessageIdString(name, messageNumber)))
			return o
		end
	},

	[0xffff0182] =
	{
		Name = "AgentDataUpdateRequest",
		Fields =
		{
			AgentId        = ProtoField.guid ("llmsg.AgentDataUpdateRequest.AgentId",        "AgentId"),
			SessionId      = ProtoField.guid ("llmsg.AgentDataUpdateRequest.SessionId",      "SessionId")
		},
		
		Decoder = function (buffer, offset, tree, pinfo)
			local messageNumber = 0xffff0182
			local name = Decoders[messageNumber].Name
			local subtree = tree:add(llmsg_protocol, buffer(offset), name)	
			local o = offset
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.AgentId,       buffer, o, 16)
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.SessionId,     buffer, o, 16)
			pinfo.cols.info:append(string.format(" %s", GetMessageIdString(name, messageNumber)))
			BodyTree:append_text(string.format(" %s", GetMessageIdString(name, messageNumber)))
			return o
		end
	},

	[0xffff0183] =
	{
		Name = "AgentDataUpdate",
		Fields =
		{
			AgentId        = ProtoField.guid   ("llmsg.AgentDataUpdate.AgentId",       "AgentId"),
			FirstName      = ProtoField.string ("llmsg.AgentDataUpdate.FirstName",     "FirstName",       base.UNICODE),
			LastName       = ProtoField.string ("llmsg.AgentDataUpdate.LastName",      "LastName",        base.UNICODE),
			GroupTitle     = ProtoField.string ("llmsg.AgentDataUpdate.GroupTitle",    "GroupTitle",      base.UNICODE),
			ActiveGroupId  = ProtoField.guid   ("llmsg.AgentDataUpdate.ActiveGroupId", "ActiveGroupId"),
			GroupPowers    = ProtoField.uint64 ("llmsg.AgentDataUpdate.GroupPowers",   "GroupPowers",     base.HEX),
			GroupName      = ProtoField.string ("llmsg.AgentDataUpdate.GroupName",     "GroupName",       base.UNICODE)
		},
		
		Decoder = function (buffer, offset, tree, pinfo)
			local messageNumber = 0xffff0183
			local name = Decoders[messageNumber].Name
			local subtree = tree:add(llmsg_protocol, buffer(offset), name)
			local o = offset

			if bitand(Header.Flags, 0x80) ~= 0 then
				buffer = ExpandZeroCode(buffer, o, length - o)
				o = 0
			end
	
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.AgentId,       buffer, o,   16)
			local len = buffer(o, 1):uint(); o = o + 1
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.FirstName,     buffer, o,  len)
			len = buffer(o, 1):uint(); o = o + 1
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.LastName,      buffer, o,  len)
			len = buffer(o, 1):uint(); o = o + 1
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.GroupTitle,    buffer, o,  len)
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.ActiveGroupId, buffer, o,   16)
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.GroupPowers,   buffer, o,    8)
			len = buffer(o, 1):uint(); o = o + 1
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.GroupName,     buffer, o,  len)
	
			pinfo.cols.info:append(string.format(" %s", GetMessageIdString(name, messageNumber)))
			BodyTree:append_text(string.format(" %s", GetMessageIdString(name, messageNumber)))
			return o
		end
	},
	
	[0xfffffffa] =
	{
		Name = "SecuredTemplateChecksumRequest",
		Fields =
		{
			Token          = ProtoField.guid  ("llmsg.SecuredTemplateChecksumRequest.Token",  "Token")
		},
		
		Decoder = function (buffer, offset, tree, pinfo)
			local messageNumber = 0xfffffffa
			local name = Decoders[messageNumber].Name
			local subtree = tree:add(llmsg_protocol, buffer(offset), name)
			local o = offset
			
			o = AddFieldToTree(subtree, Decoders[messageNumber].Fields.Token, buffer, o, 16)
			
			pinfo.cols.info:append(string.format(" %s", GetMessageIdString(name, messageNumber)))
			BodyTree:append_text(string.format(" %s", GetMessageIdString(name, messageNumber)))
			return o
		end
	},

	[0xfffffffb] =
	{
		Name = "PacketAck",
		Fields =
		{
			Id             = ProtoField.uint32("llmsg.PacketAck.Id",             "Id",          base.DEC)
		},
		
		Decoder = function (buffer, offset, tree, pinfo)
			local messageNumber = 0xfffffffb
			local name = Decoders[messageNumber].Name
			local subtree = tree:add(llmsg_protocol, buffer(offset), name)
			local o = offset
			local idString = ""
			local n = buffer(o, 1):uint(); o = o + 1
			for i = 0, n - 1, 1 do
				if i > 0 then
					idString = idString .. ", "
				end
				idString = string.format("%s%d", idString, buffer(o, 4):le_uint())
				o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.Id, buffer, o, 4)
			end
			idString = string.format("[%s]", trim(idString))
			subtree:append_text(string.format(" (%d)", n))
			pinfo.cols.info:append(string.format (" %s Id=%s", GetMessageIdString(name, messageNumber), idString))	
			BodyTree:append_text(string.format(" %s", GetMessageIdString(name, messageNumber)))
			return o
		end
	},

	[0xfffffffc] =
	{
		Name = "OpenCircuit",
		Fields =
		{
			Ip             = ProtoField.ipv4  ("llmsg.OpenCircuit.Ip",           "Ip"),
			Port           = ProtoField.uint16("llmsg.OpenCircuit.Port",         "Port",        base.DEC)
		},
		
		Decoder = function (buffer, offset, tree, pinfo)
			local messageNumber = 0xfffffffc
			local name = Decoders[messageNumber].Name
			local subtree = tree:add(llmsg_protocol, buffer(offset), name)
			local o = offset
			o = AddFieldToTree    (subtree, Decoders[messageNumber].Fields.Ip,      buffer, o, 4)
			o = AddFieldToTree_le (subtree, Decoders[messageNumber].Fields.Port,    buffer, o, 2)
			pinfo.cols.info:append(string.format(" %s", GetMessageIdString(name, messageNumber)))
			BodyTree:append_text(string.format(" %s", GetMessageIdString(name, messageNumber)))
		end
	},

	[0xfffffffd] =
	{
		Name = "CloseCircuit",
		Fields =
		{
		},
		
		Decoder = function (buffer, offset, tree, pinfo)
			local messageNumber = 0xfffffffd
			local name = Decoders[messageNumber].Name
			local subtree = tree:add(llmsg_protocol, buffer(offset), name)
			local o = offset
			
			pinfo.cols.info:append(string.format(" %s", GetMessageIdString(name, messageNumber)))
			BodyTree:append_text(string.format(" %s", GetMessageIdString(name, messageNumber)))
			return o
		end
	},
	
	-- Stub to copy from:
	[0] =
	{
		Name = "",
		Fields =
		{
		},
		
		Decoder = function (buffer, offset, tree, pinfo)
			local messageNumber = 0
			local name = Decoders[messageNumber].Name
			local subtree = tree:add(llmsg_protocol, buffer(offset), name)
			local o = offset
			
			pinfo.cols.info:append(string.format(" %s", GetMessageIdString(name, messageNumber)))
			BodyTree:append_text(string.format(" %s", GetMessageIdString(name, messageNumber)))
			return o
		end
	}
}

-- Loop through the decoders structure and add all fields to the dissector:
allFields = {}
for decoderKey, def in pairs(Decoders) do
	for fieldKey, field in pairs(def.Fields) do
		table.insert(allFields, field)
	end
end
llmsg_protocol.fields = allFields

function llmsg_protocol.dissector(buffer, pinfo, tree)
	length = buffer:len()
	if length == 0 then return end

	pinfo.cols.protocol = llmsg_protocol.name

	local subtree = tree:add(llmsg_protocol, buffer(), "LL Message System Protocol")
  
	local o = 0
	o = Decoders.Header.Decoder (buffer, o, subtree, pinfo)
	local bodyStart = o
	o = Decoders.Body.Decoder   (buffer, o, subtree, pinfo)
	local bodyLength = o - bodyStart
	
	local unknown = false
	if Decoders[Body.MessageNumber] ~= nil then
		o = Decoders[Body.MessageNumber].Decoder(buffer, o, BodyTree, pinfo)
	else
		pinfo.cols.info:append(string.format(" %s", GetMessageIdString("Unknown", Body.MessageNumber)))
		BodyTree:append_text(string.format(" %s", GetMessageIdString("Unknown", Body.MessageNumber)))
		unknown = true
	end
	
	if unknown == false and bitand(Header.Flags, 0x10) ~= 0 then
		Decoders.Ack.Decoder (buffer, o, subtree, pinfo)
	end
end

-- Start up

local udp_port = DissectorTable.get("udp.port")
udp_port:add(13007, llmsg_protocol)
