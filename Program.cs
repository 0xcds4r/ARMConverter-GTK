//
// ARM Converter by 0xcds4r (THUMB/ARM64)
// GTK edition
// Tested on arch but idk about other distro's
// 
// @date 08/01/26
//

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Gtk;

class ArmConverterApp : Window
{
    Entry inputEntry;
    TextView outputView;
    ComboBoxText archCombo;
    ComboBoxText modeCombo;

    public ArmConverterApp() : base("ARM Converter")
    {
        SetDefaultSize(700, 500);
        SetPosition(WindowPosition.Center);
        BorderWidth = 10;

        var vbox = new VBox(false, 5);
        Add(vbox);

        var inputLabel = new Label("Input (hex or ASM):");
        inputLabel.Xalign = 0;
        vbox.PackStart(inputLabel, false, false, 0);

        inputEntry = new Entry();
        inputEntry.Margin = 5;
        vbox.PackStart(inputEntry, false, false, 0);

        var outputLabel = new Label("Output:");
        outputLabel.Xalign = 0;
        vbox.PackStart(outputLabel, false, false, 0);

        outputView = new TextView();
        outputView.Editable = false;
        outputView.Margin = 5;
        outputView.HeightRequest = 250;
        var scrolled = new ScrolledWindow();
        scrolled.Add(outputView);
        vbox.PackStart(scrolled, true, true, 0);

        var controlsBox = new HBox(false, 5);
        controlsBox.Margin = 5;

        var archLabel = new Label("Architecture:");
        controlsBox.PackStart(archLabel, false, false, 0);

        archCombo = new ComboBoxText();
        archCombo.AppendText("Thumb");
        archCombo.AppendText("ARM64");
        archCombo.Active = 0;
        controlsBox.PackStart(archCombo, false, false, 0);

        var modeLabel = new Label("Mode:");
        controlsBox.PackStart(modeLabel, false, false, 0);

        modeCombo = new ComboBoxText();
        modeCombo.AppendText("Bytes → ASM");
        modeCombo.AppendText("ASM → Bytes");
        modeCombo.Active = 0;
        controlsBox.PackStart(modeCombo, false, false, 0);

        var convertButton = new Button("Convert");
        convertButton.Margin = 5;
        convertButton.Clicked += OnConvert;
        controlsBox.PackEnd(convertButton, false, false, 0);

        vbox.PackEnd(controlsBox, false, false, 0);

        DeleteEvent += (o, args) => Application.Quit();

        ShowAll();
    }

    void OnConvert(object? sender, EventArgs e)
    {
        string input = inputEntry.Text ?? "";
        input = input.Trim();
        
        if (string.IsNullOrEmpty(input))
        {
            outputView.Buffer.Text = "Error: Input is empty";
            return;
        }

        string arch = archCombo.Active switch
        {
            0 => "Thumb",
            1 => "ARM64",
            _ => "Thumb"
        };

        string mode = modeCombo.Active switch
        {
            0 => "Bytes → ASM",
            1 => "ASM → Bytes",
            _ => "Bytes → ASM"
        };

        try
        {
            if (mode == "Bytes → ASM")
            {
                byte[] bytes = ParseHex(input);
                string asm = Disassemble(bytes, arch);
                outputView.Buffer.Text = asm;
            }
            else
            {
                string hex = Assemble(input, arch);
                outputView.Buffer.Text = hex;
            }
        }
        catch (Exception ex)
        {
            outputView.Buffer.Text = $"Error: {ex.Message}";
        }
    }

    byte[] ParseHex(string hex)
    {
        hex = hex.Replace("0x", "").Replace(" ", "").Replace("-", "").Replace(",", "").ToUpper();
        if (hex.Length % 2 != 0) hex = "0" + hex;
        
        byte[] bytes = new byte[hex.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            if (!byte.TryParse(hex.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber, null, out bytes[i]))
                throw new FormatException($"Invalid hex sequence: {hex.Substring(i * 2, 2)}");
        }
        
        return bytes;
    }

    string Disassemble(byte[] code, string arch)
    {
        if (arch == "ARM64" && code.Length % 4 != 0)
            return $"Error: ARM64 requires instruction length to be multiple of 4 bytes (got {code.Length} bytes)";
        
        if (arch == "Thumb" && code.Length % 2 != 0)
            return $"Error: Thumb requires instruction length to be multiple of 2 bytes (got {code.Length} bytes)";

        string tempFile = System.IO.Path.GetTempFileName();
        File.WriteAllBytes(tempFile, code);

        try
        {
            string objdumpCmd = arch == "ARM64" ? "aarch64-linux-gnu-objdump" : "arm-none-eabi-objdump";
            
            string args = arch switch
            {
                "Thumb" => $"-D -b binary -m arm --adjust-vma=0x0 {tempFile} --disassembler-options=force-thumb",
                "ARM64" => $"-D -b binary -m aarch64 --adjust-vma=0x0 {tempFile}",
                _ => $"-D -b binary -m arm --adjust-vma=0x0 {tempFile}"
            };

            var psi = new ProcessStartInfo
            {
                FileName = objdumpCmd,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var proc = Process.Start(psi))
            {
                if (proc == null) throw new Exception($"Failed to start {objdumpCmd}");
                
                string output = proc.StandardOutput.ReadToEnd();
                string error = proc.StandardError.ReadToEnd();
                proc.WaitForExit();

                if (proc.ExitCode != 0)
                {
                    if (arch == "ARM64")
                        return "ARM64 tools not installed. Install: sudo pacman -S aarch64-linux-gnu-binutils aarch64-linux-gnu-gcc";
                    throw new Exception($"objdump error: {error}");
                }

                var lines = output.Split('\n');
                var result = new StringBuilder();
                bool foundDisassembly = false;

                foreach (var line in lines)
                {
                    if (line.Contains("Disassembly"))
                    {
                        foundDisassembly = true;
                        continue;
                    }
                    
                    if (foundDisassembly && !string.IsNullOrWhiteSpace(line))
                    {
                        result.AppendLine(line);
                    }
                }
                
                return result.Length > 0 ? result.ToString().Trim() : output.Trim();
            }
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    string Assemble(string asmText, string arch)
    {
        string tempAsm = System.IO.Path.GetTempFileName() + ".s";
        string tempObj = System.IO.Path.GetTempFileName() + ".o";

        try
        {
            File.WriteAllText(tempAsm, WrapAsm(asmText, arch));

            string compilerCmd = arch == "ARM64" ? "aarch64-linux-gnu-gcc" : "arm-none-eabi-gcc";
            string gccFlags = arch switch
            {
                "Thumb" => $"-c -mthumb -mcpu=cortex-m4 -o {tempObj} {tempAsm}",
                "ARM64" => $"-c -o {tempObj} {tempAsm}",
                _ => $"-c -mcpu=cortex-a9 -o {tempObj} {tempAsm}"
            };

            var psi = new ProcessStartInfo
            {
                FileName = compilerCmd,
                Arguments = gccFlags,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var proc = Process.Start(psi))
            {
                if (proc == null) throw new Exception($"Failed to start {compilerCmd}");
                
                string err = proc.StandardError.ReadToEnd();
                proc.WaitForExit();
                
                if (proc.ExitCode != 0)
                {
                    if (arch == "ARM64")
                        return "ARM64 tools not installed. Install: sudo pacman -S aarch64-linux-gnu-binutils aarch64-linux-gnu-gcc";
                    throw new Exception($"Compiler error: {err}");
                }
            }

            string objdumpCmd = arch == "ARM64" ? "aarch64-linux-gnu-objdump" : "arm-none-eabi-objdump";
            var objdumpPsi = new ProcessStartInfo
            {
                FileName = objdumpCmd,
                Arguments = $"-d -j .text {tempObj}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            string hexOutput;
            using (var proc = Process.Start(objdumpPsi))
            {
                if (proc == null) throw new Exception("Failed to start objdump");
                
                hexOutput = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();
            }

            var bytes = new List<byte>();
            foreach (var line in hexOutput.Split('\n'))
            {
                var trimmed = line.Trim();
                if (trimmed.Length > 0 && char.IsDigit(trimmed[0]))
                {
                    var parts = trimmed.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 1; i < parts.Length; i++)
                    {
                        string part = parts[i];
                        if ((part.Length == 4 || part.Length == 8 || part.Length == 16) && part.All(c => "0123456789abcdefABCDEF".Contains(c)))
                        {
                            for (int j = 0; j < part.Length; j += 2)
                            {
                                bytes.Add(byte.Parse(part.Substring(j, 2), System.Globalization.NumberStyles.HexNumber));
                            }
                            break;
                        }
                    }
                }
            }

            if (bytes.Count == 0)
                return "No bytes generated";

            StringBuilder sb = new StringBuilder();
            
            if (arch == "ARM64")
            {
                for (int i = 0; i < bytes.Count; i += 4)
                {
                    int end = Math.Min(i + 4, bytes.Count);
                    for (int j = end - 1; j >= i; j--)
                    {
                        sb.Append(bytes[j].ToString("X2"));
                        if (j > i) sb.Append(" ");
                    }
                    if (end < bytes.Count) sb.Append(" ");
                }
            }
            else
            {
                for (int i = 0; i < bytes.Count; i += 2)
                {
                    int end = Math.Min(i + 2, bytes.Count);
                    for (int j = end - 1; j >= i; j--)
                    {
                        sb.Append(bytes[j].ToString("X2"));
                        if (j > i) sb.Append(" ");
                    }
                    if (end < bytes.Count) sb.Append(" ");
                }
            }

            return sb.ToString();
        }
        finally
        {
            foreach (var f in new[] { tempAsm, tempObj })
                if (File.Exists(f)) File.Delete(f);
        }
    }

    string WrapAsm(string asmText, string arch)
    {
        string header = arch switch
        {
            "Thumb" => ".text\n.thumb\n.global _start\n_start:\n",
            "ARM64" => ".text\n.global _start\n_start:\n",
            _ => ".text\n.thumb\n.global _start\n_start:\n"
        };
        return header + asmText + "\n";
    }

    public static void Main(string[] args)
    {
        Application.Init();
        new ArmConverterApp();
        Application.Run();
    }
}