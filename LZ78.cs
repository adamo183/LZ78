using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
namespace LZ78
{

    public interface IFile
    {
        public void OpenFile(string filename);
        public void InsertFileName();
    }
    public interface Icompress
    {
        public void Compress();
        public void Decompress();
    }
    public class ShowProgressArgs : EventArgs
    {
        public readonly int full_size;
        public readonly int current_size;

        public ShowProgressArgs(int full_size,int current_size )
        {
            this.current_size = current_size;
            this.full_size = full_size;
        }
    }

    public class LZ78 : IFile , Icompress
    {

        
        public string SelectedFile  { get; private set; }
        public int mode { get; set; }
        public string FileExtension { get; private set; } 

        public int Dekompress_size { get; set; }
        public int Commpress_size { get; set; }

        private void showRating() => Console.WriteLine((double)Dekompress_size / (double)Commpress_size);
        public event EventHandler<ShowProgressArgs> showProgress;

        protected virtual void OnShowProgress(ShowProgressArgs e)
        {
            showProgress?.Invoke(this, e);
        }
        
        public LZ78()
        {
            SelectedFile = "";
            mode = 0;
        }

        public void Compress()
        {
            
            

            FileStream stream = File.OpenRead(SelectedFile);
            byte[] byte_tab= new byte[stream.Length];

          //  Dictionary<List<byte>, int> dictionary = new Dictionary<List<byte>, int>();
            List<byte> commpressed = new List<byte>();
            stream.Read(byte_tab, 0, byte_tab.Length);

          
              

            this.Dekompress_size = byte_tab.Length;
            var found = new List<byte>();
            int indexOfFound;

            List<KeyValuePair<int, List<byte> >> dictionary = new List<KeyValuePair<int, List<byte>>>();

            int byteNumber = 0;
            foreach (var i in byte_tab)
            {
                byteNumber++;
                OnShowProgress(new ShowProgressArgs(byte_tab.Length, byteNumber));

                var tmp = new List<byte>(found);
                tmp.Add(i);

                var isKeyExist = dictionary.Where(l => Enumerable.SequenceEqual(tmp,l.Value) ).Any();

              

                if (isKeyExist == true)
                {
                    tmp.Clear();
                    found.Add(i);
                }
                else if(isKeyExist == false)
                {
                    if (found.Count == 0)
                    {
                        indexOfFound = 0;
                        commpressed.Add(byte.MinValue);
                        commpressed.Add(byte.MinValue);
                    }
                    else
                    {
                        indexOfFound = (from x in dictionary where Enumerable.SequenceEqual(found, x.Value) select x.Key).FirstOrDefault();
                        var ByteIndex = (BitConverter.GetBytes(indexOfFound));
                        commpressed.Add(ByteIndex[1]);
                        commpressed.Add(ByteIndex[0]);
                      //  Console.WriteLine(indexOfFound);
                    }
                    commpressed.Add(i);
                    dictionary.Add(KeyValuePair.Create(dictionary.Count()+1, tmp));
                    
                    found.Clear();
                }

            }
            byte[] commpress_tab = commpressed.ToArray();
            Commpress_size = commpress_tab.Length;

            string output_name = SelectedFile.Remove(SelectedFile.Length - 4) + "LZ78" + FileExtension;
            File.WriteAllBytes(output_name, commpress_tab);
            Console.WriteLine("Commpression Rate: ");

            showRating();
        }

        public void Decompress()
        {
            FileStream stream = File.OpenRead(SelectedFile);
            byte[] byte_tab = new byte[stream.Length];

            stream.Read(byte_tab, 0, byte_tab.Length);

            List<KeyValuePair<int, List<byte>>> dictionary = new List<KeyValuePair<int, List<byte>>>();
            List<byte> decommpressed = new List<byte>();


            
            for (int i = 0; i<byte_tab.Length;i+=3)
            {
                OnShowProgress(new ShowProgressArgs(byte_tab.Length, i));
                if ((byte_tab[i] == byte.MinValue)&&(byte_tab[i+1] == byte.MinValue))
                {
                    List<byte> tmp = new List<byte>();
                    decommpressed.Add(byte_tab[i+2]);
                    tmp.Add(byte_tab[i + 2]);
                    dictionary.Add(KeyValuePair.Create(dictionary.Count() + 1, tmp));
                }
                else
                {
                    byte[] tmp_b = new byte [2];
                    tmp_b[0] = byte_tab[i+1];
                    tmp_b[1] = byte_tab[i];
                    int x = BitConverter.ToInt16(tmp_b,0);
                    try
                    {
                        List<byte> listFromIndex = (from f in dictionary where f.Key.Equals(x) select f.Value).First();
                        foreach (var s in listFromIndex)
                        {
                            decommpressed.Add(s);
                        }
                        decommpressed.Add(byte_tab[i + 2]);

                        List<byte> tmp_list = new List<byte>(listFromIndex);
                        tmp_list.Add(byte_tab[i + 2]);
                        dictionary.Add(KeyValuePair.Create(dictionary.Count() + 1, tmp_list));
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("Error during decompression.You sure that file is commpressed in LZ78?");
                        Console.ReadKey();
                        Environment.Exit(0);
                    }
                    
                        //dictionary.Where(l => l.Key == byte_tab[i]);
                    
                    
                    
                }
                
                
            }

            byte[] decommpress_tab = decommpressed.ToArray();
            string output_name = SelectedFile.Remove(SelectedFile.Length - 4) + "decomp" + FileExtension;
            File.WriteAllBytes(output_name, decommpress_tab);
        }

        public void OpenFile(string filename)
        {

        }
        public void insertMode()
        {
            Console.WriteLine("Select option:");
            Console.WriteLine("1. Compress file");
            Console.WriteLine("2. Decompress file");
            
            while (true)
            {
                string input_file = Console.ReadLine();
                switch (input_file)
                {

                    case "1":
                        this.mode = 1;
                        return;
                    case "2":
                        this.mode = 2;
                        return;

                    default:
                        Console.WriteLine("Bad choise!Try once again");
                        break;
                }
            }
        }

        public void InsertFileName()
        {
            Console.WriteLine("Enter file name:");
           
            while (SelectedFile.Length == 0)
            {
               SelectedFile = Console.ReadLine().Trim();
                if (SelectedFile.Contains('#') | SelectedFile.Contains('%') | SelectedFile.Contains('&')
               | SelectedFile.Contains('*') | SelectedFile.Contains(':') | SelectedFile.Contains('<')
               | SelectedFile.Contains('>') | SelectedFile.Contains('?'))
                {
                    Console.WriteLine("File name containts bad character. Try once again");
                    SelectedFile = "";
                    continue;
                }

                if (!(File.Exists(SelectedFile)))
                {
                    Console.WriteLine("File not exist. Try once again");
                    SelectedFile = "";
                    continue;
                }
                if(Path.GetExtension(SelectedFile) == "")
                {
                    Console.WriteLine("Do you forget about extention? Try once again");
                    SelectedFile = "";
                    continue;

                }
                // Console.WriteLine("File name cannot be empty! Try once again");

                if (SelectedFile.Length != 0)
                {
                    FileExtension = Path.GetExtension(SelectedFile);
                    return;
                }
                
            }
           
        }
        
            public void ReadDataFromFile()
            { 
           /* FileStream stream = File.OpenRead("Discord.lnk");
            byte[] fileBytes = new byte[stream.Length];

            stream.Read(fileBytes, 0, fileBytes.Length);
            stream.Close();

            
            File.WriteAllBytes("rr.lnk", fileBytes);   
            */
        }
        public void CompressFile()
        {

        }
         
    }
}
