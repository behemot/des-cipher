/**
 * DES cipher
 *
 * Author: Michał Białas <michal.bialas@mbialas.pl>
 * Since: 2012-01-12
 * Version: 0.1
 */

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using NDesk.Options;

public class Des
{	
	public static void Main(string[] args)
	{
		bool showHelp = false;
		string password = null, inputPath = null, outputPath = null, command;

		OptionSet opt = new OptionSet() {
			{"p|password=", "DES password", v => password = v},
			{"i|in=", "input file", v => inputPath = v},
			{"o|out=", "output file", v => outputPath = v},
			{"h|help", "show this message and exit", v => showHelp = v != null},
		};

		List<string> commands;
		try {
			commands = opt.Parse(args);
		} catch (OptionException e) {
			PrintError(e.Message);
			return;
		}

		if (showHelp) {
            Usage(opt);
            return;
        }

		if (0 == commands.Count) {
			PrintError("No command.");
			return;
		}
		
		command = commands[0];
		
		if ("encrypt" != command && "decrypt" != command) {
			PrintError("Invalid command.");
			return;
		}
		
		if (null == password) {
			PrintError("No password");
			return;
		}

		if (null == inputPath) {
			PrintError("The input file is not specified");
			return;
		}

		if (null == outputPath) {
			PrintError("The output file is not specified");
			return;
		}

		FileStream inputFile, outputFile;
		try {
			inputFile = File.Open(inputPath, FileMode.Open, FileAccess.Read, FileShare.None);
			outputFile = File.Open(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
		} catch (Exception e) {
			PrintError(e.Message);
			return;
		}
		
		var crypt = new DESCrypt();
		crypt.inputFile = inputFile;
		crypt.outputFile = outputFile;
		crypt.password = password.Trim();
		switch (command) {
			case "encrypt":
				crypt.Encrypt();
				break;
			case "decrypt":
				crypt.Decrypt();
				break;
		}

		Console.WriteLine("done");
		inputFile.Close();
		outputFile.Close();
	}
	
	public static void Usage(OptionSet opt)
	{
		Console.WriteLine("Usage: des.exe (encrypt|decrypt) /password:password [/in:inupt_file /out:output_file]");
        Console.WriteLine("Options:");
        opt.WriteOptionDescriptions(Console.Out);
	}
	
	public static void PrintError(string message)
	{
		Console.WriteLine("Error: {0}", message);
		Console.WriteLine ("Try des.exe /help for more information.");
	}
}

class DESCrypt
{
	const int DATA_BITS = 64;

	const int DATA_LENGTH = DATA_BITS / 8;
	
	const int KEY_BITS = 64;
	
	const int KEY_LENGH = KEY_BITS / 8;

	public FileStream inputFile { get; set; }
	 
	public FileStream outputFile { get; set; }
	 
	public string password { get; set; }
	 
	public void Decrypt()
	{
		int n;
		byte[] buffer = new byte[DATA_LENGTH];
		
		DES des = new DESCryptoServiceProvider();          
		CryptoStream encStream = new CryptoStream(outputFile, des.CreateDecryptor(GetKey(), GetKey()), CryptoStreamMode.Write);

		while ((n = inputFile.Read(buffer, 0, buffer.Length)) > 0) {
			encStream.Write(buffer, 0, n);
		}
		
		encStream.Close();
	}
	 
	public void Encrypt()
	{
		int n;
		byte[] buffer = new byte[DATA_LENGTH];
		
		DES des = new DESCryptoServiceProvider();          
		CryptoStream encStream = new CryptoStream(outputFile, des.CreateEncryptor(GetKey(), GetKey()), CryptoStreamMode.Write);
		
		while ((n = inputFile.Read(buffer, 0, buffer.Length)) > 0) {
			encStream.Write(buffer, 0, n);
		}

		encStream.Close();
	}
	
	private byte[] GetKey()
	{
		var tmp = new byte[KEY_LENGH];
		byte[] bytes = Encoding.ASCII.GetBytes(password);
		for (int i = 0; i < KEY_LENGH && i < bytes.Length; ++i) {
			tmp[i] = bytes[i];
		}
		return tmp;
	}
}
