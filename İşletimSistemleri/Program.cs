using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace İşletimSistemleri
{
    class Program
    {
        static object obj = new object();
        static int sayi = 0;
        static int[] dizi = new int[11];


        static void yaz(object indis)
        {
            for (int i = 0; i < 10000; i++)
            {
                Console.Write("y"); //Kritik Alan

                lock (obj)  //THREAD SAFE kod yazılacağı zaman yazılır

                    sayi++;

                //dizi[(int)indis]++; //dizi değişkenin 10.indisini 10.000 arttırıyor.
            }
        }
        //-----------------------------------------------------------------------------------

        static void hesapla()
        {   //
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < 110000; i++)
            {
                sayi++;
            }
            sw.Stop();
            Console.WriteLine("hesapla:{0}", sw.ElapsedTicks);
        }

        //Race Condition-------------------------
        static int sharedInt = 0;
        private static void azalt()
        {
            for (int i = 0; i < 1000000; i++)
            {
                sharedInt--;
            }
        }

        private static void arttır()
        {
            for (int i = 0; i < 1000000; i++)
            {
                sharedInt++;
            }
        }

        static int arttirSayac = 0;

        static void arttir()
        {
            for (int i = 0; i < 100000; i++)
            {
                Thread.Sleep(1);
                sayac += i;
            }
        }

        //-------------------------------------------------------------------------- DEADLOCK
        static object o1 = new object();
        static object o2 = new object();
        static void y1()
        {
            lock (o1)
            {
                Console.WriteLine("1");
                Thread.Sleep(10000);
                lock (o2)
                    Console.WriteLine("2");
            }
        }

        static void y2()
        {
            lock (o2)
            {
                Console.WriteLine("3");
                Thread.Sleep(10000);
                lock (o1)
                    Console.WriteLine("4");
            }
        }

        //-------------------------------------------------------------------------- Semaphore // alttaki semaphore kodları daha basit anlatıyor, bu çok karmaşık

        //Main thread beklerken bu fonksiyon çalışıyor
        private static Semaphore _pool;
        private static int _padding;
        private static void Worker(object num) //Semaphore 
        {
            Console.WriteLine("Thread {0} begins" + "and waits for the semaphore", num);
            _pool.WaitOne();//K.A. girerken Semaphore un WaitOne fonk. kullanılır.
                            //Hangi Thread in seçileceği rastgele belirlenir.

            //Threadler _pool.WaitOne() da bekleyecekler. Aksi belirtilmediği sürece
            // WaitOne dan ileri gidemeyecekler.
            //Gidememesinin nedeni Semaphore(0,3); yazmış olmamız.
            //Burada 0 ile biz göndermediğimiz sürece gönderme demiş olduk.

            //Main Thread Thread.Sleep(500); komutunu bitirdeikten sonra _pool.Relase(3);
            // komutu ile Semaphore a 3 tane Thread alabileceğimizi söyledik.
            //Bekleyen 5 thread den 3 tanesini içeriye aldık. Ancak hangilerini aldığını bilmiyoruz çünkü random gelir.


            /*  Lock mekanizmasında WaitOne() ve Relase() in karşılığı 
             *  
             *  lock(obj)
             *  {  ---> WaitOne() 
             *  Thread.Sleep(121312);
             *  }  ---> Relase()
             * */
            int padding = Interlocked.Add(ref _padding, 100);
            Console.WriteLine("Thread {0} enters the semaphore", num);
            Thread.Sleep(100 + padding);
            Console.WriteLine("Thread {0} relase the semaphore", num);
            Console.WriteLine("Thread {0} previous semaphore count: {1}", num, _pool.Release());
            //K.A dan çıkarken Relase() fonk. kullanılır.
        }

        //------------------------------- Interlocked

        static int sayac = 0;
        private static void arttirInterlocked()
        {
            for (int i = 0; i < 1000; i++)
            {
                //Interlocked.Add(ref sayac, 10);         // belirtilen değeri(10) sayaca değişkenine ekler
                //Interlocked.Increment(ref sayac);       // sayacı atomik bir şekilde bir arttırır.
                //Interlocked.Exchange(ref sayac, 10);    // sayacın değerini belirtilen değerle(10) değiştirir ve eski değeri döndürür.

            }
        }

        //--------------------------Interlocked Sınav Sorusu (2022)

        static int sayacBir = 0;
        static int sayacSıfır = 0;

        static void doluBosKontrol(int[] dizi, int baslangic, int bitis)
        {
            for (int i = baslangic; i < bitis; i++)
            {
                if (dizi[i] == 1)
                {
                    Interlocked.Increment(ref sayacBir);
                }
                else
                {
                    Interlocked.Increment(ref sayacSıfır);
                }
            }

        }


        //------------------------------ Semaphore

        static int semSayac = 5; // thread poola girecek thread sayısı
      
        static void WaitONE() //kendi WaitOne ve Relase metotlarımız yazdık
        {
            while (semSayac <= 0) Interlocked.Add(ref semSayac, -1);
        }
        static void RELASE()
        {
            Interlocked.Add(ref semSayac, +1);
        }

        static Semaphore sem = new Semaphore(0,3); // başlangıçta kritik alanda 0 thread olacak daha sonra ise kritik alana 3 tane thread alabileceğiz.
        static void yazSem()
        {

            Console.WriteLine(Thread.CurrentThread.Name + "Bekliyor");
            //sem.WaitOne(); // semaphore girme izni
            WaitONE();
            Console.WriteLine("Semaphore İçerisindeyiz");
            Thread.Sleep(5000);

            //sem.Release();
            RELASE();
            Console.WriteLine("Semaphoredan Çıktık");
        }


        //------------------ AutoResetEvent BURASI FİNAL KONUSU
        static void MyReadThreadProc()
        {
            while (true)
            {
                myResetEvent.WaitOne();
                Console.WriteLine(number);
            }
        }
        static AutoResetEvent myResetEvent = new AutoResetEvent(false);
        static int number;

        static void Main(string[] args)
        {

            #region Thread oluşturma
            //Thread t = new Thread(yaz); // t --> child thread
            //t.Start();
            //Thread[] ts = new Thread[10];
            /*
            for (int i = 0; i < 1000; i++)
            {
                Console.Write("x"); //Kritik Alan
            }*/
            //yukarıdaki yaz fonksiyonuyla aynı anda çalışırsa
            // Bu iki thread aynı anda çalışırken RACE CONDİTİON problemi oluşabilir
            //Çünkü 1 ekran var ve 2 tane Thread bu ekrana erişiyor 
            #endregion

            #region Race Condition Örnek 
            //Race Condition: Birden fazla processin aynı anda aynı threade ulaşmaya çalışması durumu

            /*   Thread[] th = new Thread[2];

               th[0] = new Thread(arttır);
               th[1] = new Thread(azalt);

               for (int i = 0; i < th.Length; i++)
               {
                   th[i].Start(); //Kritik Alan
               }
               for (int i = 0; i < th.Length; i++)
               {
                   th[i].Join(); //Kritik Alan
               }
               Console.WriteLine("int:" + sharedInt); */

            // race condition 2. örnek
            /* Thread t1 = new Thread(arttir);
             Thread t2 = new Thread(arttir);

             // Thread'leri başlatıyoruz
             t1.Start();
             t2.Start();

             // İki thread'in de bitmesini bekliyoruz
             t1.Join();
             t2.Join();

             Console.WriteLine("Sonuç: " + arttirSayac);*/

            #endregion

            #region THREAD UNSAFE kod

            /*
            for (int i = 0; i < 1000; i++)
            {
                sayi++;
            }
            Thread.Sleep(100);
            //Alttaki sorunu çözmek için 100ms duraklatıldı
            //Bu kod THREAD UNSAFE bir koddur.
            //THREAD UNSAFE kod: kodlamadan umulanın bulunamaması anlamına gelir.
            //THREAD UNSAFE kodlar kolay yakalanamazlar ve çalıştıklarında düzgün sonuç verir.


            Console.WriteLine(sayi);

            //sonucun 2000 değilde 1000 çıkmasının nedeni t.Start() dedikten sonra Thread,
            //yaz fonk. gidene kadar Main Thread'deki for işlemi tamamlanıyor ve ekrana yazdırma işlemi gerçekleşiyor
            //Bu sorunu çözmek için Main Theread biraz yavaşlamalı

            //THREAD UNSAFE kod çalışmaz, bozuk veya error veren kod demek değildir.
            //Problemli kod demektir. Ne zaman ne şekilde hata üreteceği bilinmez
            */
            #endregion

            #region JOİN UNSAFE KOD
            /*
            for (int i = 0; i < 1000; i++)
            {
                sayi++;
            }
            t.Join();
            Console.WriteLine(sayi);

            //t.Join() ile t Thread'inin beklenmesi sağlanır.
            //Join komutu sayesinde Main Thread, for işlemini bitirdikten sonra kendisini bloke eder
            //ve t Threadi işinin bitirince Main Thread uyandırılır
            //THREAD UNSAFE koddur.
            */
            #endregion

            #region ROUND ROBİN

            /*

            for (int i = 0; i < 10; i++)
            {
                ts[i] = new Thread(yaz);
                ts[i].Start();
            }
            for (int i = 0; i < 10000; i++)
            {
                sayi++; //Kritik Alan
            }
            for (int i = 0; i < 10; i++)
            {
                ts[i].Join(); 
            }
            Console.WriteLine(sayi);
            */
            //Bu bilgisayar 4 çekirdekli ancak 11 tane Thread kullanıldığı için Round Robin yapısında çalışacak
            //Dolaysıyla burada RACE CONDİTİON problemi oldu. 
            //1 tane thread var ve 1 sistem kaynağına erişmeye çalışıyorlar.

            //Race Condition çözülmesi için
            //önce kritik Alan belirlenmesi gerekir.
            //daha sonra bu kritik alanların tamir edilmesi gerekir.

            #endregion

            #region THREAD SAFE
            /*
            Thread[] ts = new Thread[10];
            for (int i = 0; i < 10; i++)
            {
                ts[i] = new Thread(yaz);
                ts[i].Start();
            }
            for (int i = 0; i < 10000; i++)
            {
                lock (obj) 
                // lock(obj) Çalışma mantığı: 11 THREAD birden lock'ı çalıştırır.
                // Bu THREAD lerden sadece 1 tanesi yarışı kazanır.
                //Kazanan THREAD obj değişkenine kitlenmiş olur. 
                //Diğer THREADler bu durumda bekliyor. Fonksiyondan çıkıldıktan sonra kilit THREAD
                //tarafından kilit açılıyor. Geriye kalan 10 THREAD den yine 1 tanesi yarışı kazanıyor.
                //Bu şekilde THREADLER bitene kadar devam ediyor.
                //SENKRONİZASYON DEĞİŞKENLERİ KİLİ DEĞİŞKENLERİ MANTIĞIYLA KULLANILIR

                sayi++; //Kritik Alan
            }
            for (int i = 0; i < 10; i++)
            {
                ts[i].Join();
            }
            Console.WriteLine(sayi);
            */
            #endregion

            #region 1Thread vs 11Thread    Lock farkı
            /*
            Stopwatch sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < 10; i++)
            {
                ts[i] = new Thread(yaz);
                ts[i].Start();
            }
            for (int i = 0; i < 10000; i++)
            {
                lock (obj)
                sayi++; //Kritik Alan
            }
            for (int i = 0; i < 10; i++)
            {
                ts[i].Join();
            }
            sw.Stop();
            hesapla();

            Console.WriteLine("11 thread:{0}",sw.ElapsedTicks);
            Console.WriteLine(sayi);

            // Bu kadar büyük performans kaybı yaşanmasının sebebi lock kullanmamızdır.
            //Yani lock değişkenleri performans kaybı oluşturuyor.
            //Ayrıca Lock değişkenleri DEAD LOCK oluşturur.

            //Not:Lock değişkenleriyle senkronizasyon problemleri çözülür.*/
            //Bunun yan etkisi ise performans kaybı ve DEADLOCK dır.
            #endregion

            #region 1Thread vs 11Thread Dizi farkı
            /*
            Stopwatch sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < 10; i++)
            {
                ts[i] = new Thread(yaz);
                ts[i].Start(i);
                //ts[i].Start(i) komutundan belirtilen i objesni parametre olarak yaz fonk. gönderiyor
                //ve i nin değeri dizinin indisi olarak kabul ediliyor.
                //Bu indisin değeri 10.000 arttırılıyor.
            }
            for (int i = 0; i < 10000; i++)
            {
                dizi[10]++;
            }
            for (int i = 0; i < 10; i++)
            {
                ts[i].Join();
            }
            sw.Stop();
            Console.WriteLine("11 thread:{0}", sw.ElapsedTicks);

            for (int i = 0; i < dizi.Length; i++)
            {
                Console.WriteLine("Dizinin"+i+".Elemanı"+dizi[i]);
            }

            //Çıktıya göre; dizinin elemanlarını 110.000 tane arttırılmış oldu.
            //Ancak her Thread kendisine ayrılan yerde çalışmış oldu.
            //Dolaysıyla performans daha iyi hale geldi.

            //Bu kodda KRİTİK ALAN vardır ancak RACE CONDİTİON yoktur.
            //RACE CONDİTİON olmadığı için KRİTİK ALAN zararsız olmuş oluyor.


            */
            #endregion

            #region Dead Lock

            /*
            Thread th = new Thread(y1);
            th.Start();
            th = new Thread(y2);
            th.Start();*/
            //Burada DEADLOCK durumu oluşur.
            //Senkronizasyon sorunu vardır.

            #endregion

            #region Semaphore
            /*
            lock (obj)
            {
                Thread.Sleep(1200000);
            }
            //Thread, bu koda girdiğinde beklemeye başlayacak ve diğer Threadler,de
            //bu Threadi beklemeye başlayacak. Böyle bir durumda obj nin kilidi, sadece 
            //kilidi atan Thread kaldırabilir.

            //Ancak semaphore da kilidi başka bir Thread kaldırabilir.


            _pool = new Semaphore(0, 3);
            //pool adında semopher değişkeni tanımlanır. Ardından Main Thread de
            // _pool = new Semaphore(0, 3); komutu ile Semaphore oluştuluruyor

            //Semaphore 2 parametre alır
            //1.Parametre: İlk başta kaç Thread alınacak
            //2.Parametre: Kritik alanda bulunan Thread sayısı.

            for (int i = 0; i <= 5; i++) //5 tane Thread oluşturuluyor
            {
                //Thread tr = new Thread(Worker);
                Thread tr = new Thread(new ParameterizedThreadStart(Worker));

                tr.Start(i);
                //5 thread Worker fonksiyonuna t.Start(i) ile gönderiliyor

                //Eğer Threade 1 den fazla parametre gönderilmek istenirse tr.Start(i,j)
                //şeklinde gönderilemez. Fonksiyonun parametre kısmında başka bir class tanımlanarak
                //o class ın içinde istenilen parametreler gönderilir. Parametredeki değerin tipi de o classs olur 
            }
            Thread.Sleep(500); // Main Thread in yarım saniye beklemesini sağlıyor
            //Main thread beklerken bu Worker fonksiyonu çalışıyor

            _pool = new Semaphore(0, 3);
            Console.WriteLine("Main Thread calls Relase(3).");

            Console.WriteLine("Main Thread Çıkıyor");*/
            #endregion

            #region Semaphore V2

            /* for (int i = 0; i < 6; i++)
             {
                 Thread t = new Thread(yazSem);
                 t.Name = "thread"+ i;
                 t.Start();
             }*/


            #endregion

            #region Interlocked


            /*Thread t1 = new Thread(arttirInterlocked);
            Thread t2 = new Thread(arttirInterlocked);

            // Thread'leri başlatıyoruz
            t1.Start();
            t2.Start();

            // İki thread'in de bitmesini bekliyoruz
            t1.Join();
            t2.Join();

            Console.WriteLine("Sonuç: " + sayac);*/

            #endregion

            #region Interlocked Final Sınav Sorusu (2022)

            // 100000 elemanlı dizi var. bu dizi 1 ve 0 lardan oluşmakta -> [1,1,0,1,1,...] 0 -> DOLU // 1 -> BOŞ
            // her eleman bir clustera bakmakta
            // dizinin doluluk oranını 4 thread ile bul.

            int[] dizi = new int[100];
            Random random = new Random();

            for (int i = 0; i < dizi.Length; i++)
            {
                dizi[i] = random.Next(2); // 0 veya 1 değeri alır
            }
            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine(dizi[i]);
            }
            Thread th1 = new Thread(() => doluBosKontrol(dizi, 0,24));
            Thread th2 = new Thread(() => doluBosKontrol(dizi, 25, 49));
            Thread th3 = new Thread(() => doluBosKontrol(dizi, 50,74));
            Thread th4 = new Thread(() => doluBosKontrol(dizi,75,99));

            th1.Start();
            th2.Start();
            th3.Start();
            th4.Start();

            th1.Join();
            th2.Join();
            th3.Join();
            th4.Join();

            Console.WriteLine("Sıfır:"+ sayacSıfır);
            Console.WriteLine("Bir:"+ sayacBir);

            #endregion


            //threadlere isim verilebiir.
            //th.Name="racecondition"+i.ToString();
            //bütün threadler foreground dır.
            //background thread main thread görevini tamamladığı zaman otomatik olarak kapatılır. Process kapanmaz
            //ancak main thread foreground thread.
            //foreground threadler backgrounda dönüştürüldüğü zaman ikisi arassındaki tek fark background un otomatik olarak kapatması.
            //herbir thread 1 defa kullanılır.

            //SEMAPHORE 
            //Kritik alanda 1 den fazla thread olabilmesine olanak sağlar
            //Kiliti kim attıysa o açar. Semaphoreda isteyen herkes açar. 
            //Çok yavaş. lock dan 50 kat civarında yavaş.

            // Console.ReadLine();


            //Thread thr = new Thread((object obj) => { Console.WriteLine(obj); });

            // thr.Start();

            // thr.Join();

            //thr.Start();




            /* Thread myReaderThread = new Thread(MyReadThreadProc);
             myReaderThread.Name = "ReaderThread"; 
             myReaderThread.Start();
             for (int i = 1; i <= 3; i++)
             {
                 Console.WriteLine(i);
                 number = i;
                 myResetEvent.Set();
                 Thread.Sleep(1);
             }*/


            /* object locker1 = new object();
             object locker2 = new object();
             Console.WriteLine("1");
             new Thread(() =>
             {
                 lock (locker2)
                 {
                     Console.WriteLine("2");
                     Thread.Sleep(2000);
                     lock (locker1) ;
                     Console.WriteLine("3");
                 }
             }).Start();
             lock (locker2) // locker1
             {
                 Console.WriteLine("4");
                 Thread.Sleep(2000);
                 lock (locker2) ; // locker2
                 Console.WriteLine("5");
             }
             Console.WriteLine("6");*/




            Console.ReadLine();




        }
        
    }
}
