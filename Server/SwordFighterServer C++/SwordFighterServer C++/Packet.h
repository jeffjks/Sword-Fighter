#pragma once
#include <vector>
#include <string>
#include <iostream>

using namespace std;


/// <summary>Sent from server to client.</summary>
enum ChatServerPackets
{
    getUserId = 1,
    chatServerMessage
};

/// <summary>Sent from client to server.</summary>
enum ChatClientPackets
{
    sendUserId = 1,
    chatClientMessage
};

// https://sanghun219.tistory.com/192?category=894929

class Packet
{
private:
	vector<uint8_t> buffer;
	//char *readableBuffer;
	unsigned int readPos;

	vector<uint8_t> intToBytesVector(int paramInt)
	{
		vector<uint8_t> arrayOfByte(4);
		for (int i = 0; i < 4; i++)
			arrayOfByte[3 - i] = (paramInt >> (i * 8));
		return arrayOfByte;
	}

public:
	/// <summary>Creates a new empty packet (without an ID).</summary>
	Packet() {
		buffer = vector<uint8_t>(); // Intitialize buffer
		readPos = 0; // Set readPos to 0
	}

	/// <summary>Creates a new packet with a given ID. Used for sending.</summary>
	/// <param name="_id">The packet ID.</param>
	Packet(int _id) {
		buffer = vector<uint8_t>(); // Intitialize buffer
		readPos = 0; // Set readPos to 0

		Write(_id); // Write packet id to the buffer
	}

	/// <summary>Creates a packet from which data can be read. Used for receiving.</summary>
	/// <param name="_data">The bytes to add to the packet.</param>
	Packet(const char* _data, int _length)
	{
		buffer = vector<uint8_t>(); // Intitialize buffer
		readPos = 0; // Set readPos to 0

		SetBytes(_data, _length);
	}

	/// <summary>Sets the packet's content and prepares it to be read.</summary>
	/// <param name="_data">The bytes to add to the packet.</param>
	void SetBytes(const char* _data, int _length)
	{
		Write(_data, _length);
		//readableBuffer = &buffer[0];
	}

	/// <summary>Inserts the length of the packet's content at the start of the buffer.</summary>
	void WriteLength()
	{
		vector<uint8_t> length_vec = intToBytesVector(buffer.size());
		buffer.insert(buffer.begin(), length_vec.begin(), length_vec.end());
		//buffer.InsertRange(0, BitConverter.GetBytes(buffer.Count)); // Insert the byte length of the packet at the very beginning
	}

	/// <summary>Gets the packet's content in array form.</summary>
	const char* ToArray()
	{
		char* readableBuffer = new char[buffer.size()]; // init this with the correct size
		copy(buffer.begin(), buffer.end(), readableBuffer);
		return readableBuffer;
	}

	/// <summary>Gets the length of the packet's content.</summary>
	int Length()
	{
		return buffer.size(); // Return the length of buffer
	}

	/// <summary>Gets the length of the unread data contained in the packet.</summary>
	int UnreadLength()
	{
		return Length() - readPos; // Return the remaining length (unread)
	}

	/// <summary>Resets the packet instance to allow it to be reused.</summary>
	/// <param name="_shouldReset">Whether or not to reset the packet.</param>
	void Reset(bool _shouldReset = true)
	{
		if (_shouldReset)
		{
			buffer.clear(); // Clear buffer
			//readableBuffer = NULL; // TODO : memory
			readPos = 0; // Reset readPos
		}
		else
		{
			readPos -= 4; // "Unread" the last read int
		}
	}

	/// <summary>Adds a byte to the packet.</summary>
	/// <param name="_value">The byte to add.</param>
	void Write(char _value)
	{
		buffer.push_back(_value);
	}

	/// <summary>Adds an array of bytes to the packet.</summary>
	/// <param name="_value">The byte array to add.</param>
	void Write(const char* _value, int len_)
	{
		for (int i = 0; i < len_; ++i) {
			buffer.push_back(_value[i]);
		}
	}

	/// <summary>Adds a short to the packet.</summary>
	/// <param name="_value">The short to add.</param>
	void Write(short _value)
	{
		for (int i = 0; i < sizeof(_value); ++i) {
			buffer.push_back(_value >> ((sizeof(_value) - 1 - i) * 8));
		}
	}
	/// <summary>Adds an int to the packet.</summary>
	/// <param name="_value">The int to add.</param>
	void Write(int _value)
	{
		for (int i = 0; i < sizeof(_value); ++i) {
			buffer.push_back(_value >> ((sizeof(_value) - 1 - i) * 8));
		}
	}
	/// <summary>Adds a long to the packet.</summary>
	/// <param name="_value">The long to add.</param>
	void Write(long _value)
	{
		for (int i = 0; i < sizeof(_value); ++i) {
			buffer.push_back((uint8_t) _value >> ((sizeof(_value) - 1 - i) * 8));
		}
	}
	/// <summary>Adds a bool to the packet.</summary>
	/// <param name="_value">The bool to add.</param>
	void Write(bool _value)
	{
		for (int i = 0; i < sizeof(_value); ++i) {
			buffer.push_back(_value >> ((sizeof(_value) - 1 - i) * 8));
		}
	}
	/// <summary>Adds a string to the packet.</summary>
	/// <param name="_value">The string to add.</param>
	void Write(string _value)
	{
		Write((int)_value.length());
		vector<uint8_t> vec(_value.begin(), _value.end());
		buffer.insert(buffer.end(), vec.begin(), vec.end());
		/*
		Write(_value.Length); // Add the length of the string to the packet
		buffer.AddRange(Encoding.ASCII.GetBytes(_value)); // Add the string itself

		std::string str = "hello";
		BYTE byte[6];   // null terminated string;
		strcpy(byte, str.c_str());  // copy from str to byte[]
		*/
	}

	/// <summary>Reads a byte from the packet.</summary>
	/// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
	char ReadByte(bool _moveReadPos = true)
	{
		if (buffer.size() > readPos)
		{
			// If there are unread bytes
			char _value = buffer[readPos]; // Get the byte at readPos' position
			if (_moveReadPos)
			{
				// If _moveReadPos is true
				readPos += 1; // Increase readPos by 1
			}
			return _value; // Return the byte
		}
		else
		{
			throw exception("Could not read value of type 'byte'!");
		}
	}

	/// <summary>Reads an array of bytes from the packet.</summary>
	/// <param name="_length">The length of the byte array.</param>
	/// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
	char* ReadBytes(int _length, bool _moveReadPos = true)
	{
		if (buffer.size() > readPos)
		{
			char* readableBuffer = new char[_length];
			copy(buffer.begin() + readPos, buffer.begin() + readPos + _length, readableBuffer);
			if (_moveReadPos)
			{
				// If _moveReadPos is true
				readPos += _length; // Increase readPos by _length
			}

			return readableBuffer; // Return the bytes
		}
		else
		{
			throw exception("Could not read value of type 'byte[]'!");
		}
	}

	/// <summary>Reads a short from the packet.</summary>
	/// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
	short ReadShort(bool _moveReadPos = true)
	{
		if (buffer.size() > readPos)
		{
			// If there are unread bytes
			short _value = 0;
			for (int i = 0; i < sizeof(short); ++i) { // Convert the bytes to an short
				_value += buffer[readPos + i] << (sizeof(short) - 1 - i) * 8;
			}
			if (_moveReadPos)
			{
				// If _moveReadPos is true and there are unread bytes
				readPos += sizeof(short); // Increase readPos by 2
			}
			return _value; // Return the short
		}
		else
		{
			throw new exception("Could not read value of type 'short'!");
		}
	}

	/// <summary>Reads an int from the packet.</summary>
	/// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
	int ReadInt(bool _moveReadPos = true)
	{
		if (buffer.size() > readPos)
		{
			// If there are unread bytes
			short _value = 0;
			for (int i = 0; i < sizeof(int); ++i) { // Convert the bytes to an int
				_value += buffer[readPos + i] << (sizeof(int) - 1 - i) * 8;
			}
			if (_moveReadPos)
			{
				// If _moveReadPos is true
				readPos += sizeof(int); // Increase readPos by 4
			}
			return _value; // Return the int
		}
		else
		{
			throw new exception("Could not read value of type 'int'!");
		}
	}

	/// <summary>Reads a long from the packet.</summary>
	/// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
	long ReadLong(bool _moveReadPos = true)
	{
		if (buffer.size() > readPos)
		{
			// If there are unread bytes
			long _value = 0;
			for (int i = 0; i < sizeof(long); ++i) { // Convert the bytes to an long
				_value += buffer[readPos + i] << (sizeof(long) - 1 - i) * 8;
			}
			if (_moveReadPos)
			{
				// If _moveReadPos is true
				readPos += sizeof(long); // Increase readPos by 8
			}
			return _value; // Return the long
		}
		else
		{
			throw new exception("Could not read value of type 'long'!");
		}
	}

	/// <summary>Reads a bool from the packet.</summary>
	/// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
	bool ReadBool(bool _moveReadPos = true)
	{
		if (buffer.size() > readPos)
		{
			// If there are unread bytes
			bool _value = buffer[readPos];
			if (_moveReadPos)
			{
				// If _moveReadPos is true
				readPos += sizeof(bool); // Increase readPos by 1
			}
			return _value; // Return the bool
		}
		else
		{
			throw new exception("Could not read value of type 'bool'!");
		}
	}

	/// <summary>Reads a string from the packet.</summary>
	/// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
	string ReadString(bool _moveReadPos = true)
	{
		try
		{
			int _length = ReadInt(); // Get the length of the string
			string str;

			int i = 0;
			while (i < _length) {
				int char_size = 0;

				if ((buffer[readPos + i] & 0b11111000) == 0b11110000) { // 11110
					char_size = 4;
				}
				else if ((buffer[readPos + i] & 0b11110000) == 0b11100000) { // 1110
					char_size = 3;
				}
				else if ((buffer[readPos + i] & 0b11100000) == 0b11000000) { // 110
					char_size = 2;
				}
				else if ((buffer[readPos + i] & 0b10000000) == 0b00000000) { // 0
					char_size = 1;
				}
				else {
					char_size = 1;
				}
				str.append(string(buffer.begin() + readPos + i, buffer.begin() + readPos + char_size + i));

				i += char_size;
			}

			string _value = str; // Convert the bytes to a string
			if (_moveReadPos && _value.length() > 0)
			{
				// If _moveReadPos is true string is not empty
				readPos += _length; // Increase readPos by the length of the string
			}
			return _value; // Return the string
		}
		catch (exception e)
		{
			throw new exception("Could not read value of type 'string'!");
		}
	}
};
