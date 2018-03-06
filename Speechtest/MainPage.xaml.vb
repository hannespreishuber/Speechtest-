' Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Runtime.Serialization.Json
Imports System.Text
Imports Newtonsoft.Json
Imports Windows.Media.Capture
Imports Windows.Media.MediaProperties
Imports Windows.Storage
Imports Windows.Storage.Streams
Imports Windows.UI
''' <summary>
''' Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
''' </summary>
Public NotInheritable Class MainPage
    Inherits Page
    Dim capture As New MediaCapture
    Dim buffer As New InMemoryRandomAccessStream

    Dim onoff As Boolean = True

    Private Async Function Ellipse_TappedAsync(sender As Object, e As TappedRoutedEventArgs) As Task
        If onoff Then
            recorder.Fill = New SolidColorBrush(Colors.Red)

            Await capture.StartRecordToStreamAsync(MediaEncodingProfile.CreateWav(AudioEncodingQuality.Low), buffer)
        Else
            recorder.Fill = New SolidColorBrush(Colors.Green)
            Await capture.StopRecordAsync()
            Using dataReader = New DataReader(buffer.GetInputStreamAt(0))
                Await dataReader.LoadAsync(buffer.Size)
                Dim b(buffer.Size - 1) As Byte
                dataReader.ReadBytes(b)
                Dim saveFile = Await KnownFolders.MusicLibrary.CreateFileAsync(
                        "audio.wav", CreationCollisionOption.ReplaceExisting)
                Await Windows.Storage.FileIO.WriteBytesAsync(saveFile, b)

            End Using
        End If
        onoff = Not onoff
    End Function

    Private Async Sub Button_ClickAsync(sender As Object, e As RoutedEventArgs)
        recorder.Fill = New SolidColorBrush(Colors.Yellow)

        Using client = New HttpClient()
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "1b877c4bbaa24b26acb7019ffbdb95af")
            Dim UriBuilder = New UriBuilder("https://api.cognitive.microsoft.com/sts/v1.0")
            UriBuilder.Path += "/issueToken"
            Dim result = Await client.PostAsync(UriBuilder.Uri.AbsoluteUri, Nothing)
            text1.Text = Await result.Content.ReadAsStringAsync()



            Dim request As HttpWebRequest = HttpWebRequest.Create("https://speech.platform.bing.com/speech/recognition/interactive/cognitiveservices/v1?language=de-de&format=detailed")
            request.Accept = "application/json;text/xml"
            request.Method = "POST"
            request.ContentType = "audio/wav; codec=audio/pcm; samplerate=16000"
            request.Headers("Authorization") = "Bearer " + text1.Text
            Using requestStream = Await request.GetRequestStreamAsync()
                Dim f = Await KnownFolders.MusicLibrary.GetFileAsync("audio.wav")
                Using fs = Await f.OpenSequentialReadAsync()
                    Dim buffer(1024) As Byte
                    Using sr = New BinaryReader(fs.AsStreamForRead())
                        Dim lang = sr.BaseStream.Length
                        While lang > 0
                            buffer = sr.ReadBytes(1024)
                            requestStream.Write(buffer, 0, buffer.Length)
                            lang -= 1024
                        End While
                    End Using
                End Using
                requestStream.Flush()
            End Using


            Using response = Await request.GetResponseAsync

                Using sr = New StreamReader(response.GetResponseStream())
                    text1.Text = sr.ReadToEnd()
                End Using
            End Using
        End Using

        recorder.Fill = New SolidColorBrush(Colors.Blue)

        Dim daten = New RestText
        daten = JsonConvert.DeserializeObject(Of RestText)(text1.Text)
        Dim msg As New ServiceMeldungen
        msg.Datum = Date.Now
        msg.Meldung = daten.Nbest.First.Display
        msg.Id = 0
        CreateMessageAsync(msg)

    End Sub

    Private Async Function MainPage_LoadedAsync(sender As Object, e As RoutedEventArgs) As Task Handles Me.Loaded
        Dim settings = New MediaCaptureInitializationSettings
        settings.StreamingCaptureMode = StreamingCaptureMode.Audio
        Await capture.InitializeAsync(settings)
    End Function
    Async Sub CreateMessageAsync(msg As ServiceMeldungen)

        Dim client = New HttpClient()
        Dim json = JsonConvert.SerializeObject(msg)
        Dim Content = New StringContent(json, Encoding.ASCII, "application/json")
        Content.Headers.ContentType = New MediaTypeHeaderValue("application/json")
        Dim response = Await client.PostAsync(
           "https://iotservice2018.azurewebsites.net/api/servicemeldungens", Content)
        response.EnsureSuccessStatusCode()

    End Sub

    Private Sub Close_Click(sender As Object, e As RoutedEventArgs)
        Application.Current.Exit()
    End Sub
End Class
'https://azure.microsoft.com/en-us/try/cognitive-services/my-apis/?apiSlug=speech-api&country=Germany&allowContact=true
'https://api.cognitive.microsoft.com/sts/v1.0
'1b877c4bbaa24b26acb7019ffbdb95af
'{"RecognitionStatus":"Success","Offset":3200000,"Duration":16500000,"NBest":[{"Confidence":0.7879451,"Lexical":"ist alina zu jung","ITN":"ist Alina zu jung","MaskedITN":"ist Alina zu jung","Display":"Ist Alina zu jung."},{"Confidence":0.7879451,"Lexical":"wie ist alina zu jung","ITN":"Wie ist Alina zu jung","MaskedITN":"Wie ist Alina zu jung","Display":"Wie ist Alina zu jung?"},{"Confidence":0.7835977,"Lexical":"wer ist alina zu jung","ITN":"Wer ist Alina zu jung","MaskedITN":"Wer ist Alina zu jung","Display":"Wer ist Alina zu jung?"},{"Confidence":0.5198187,"Lexical":"ist alina ziehung","ITN":"ist Alina Ziehung","MaskedITN":"ist Alina Ziehung","Display":"Ist Alina Ziehung."},{"Confidence":0.5198187,"Lexical":"wie ist alina ziehung","ITN":"Wie ist Alina Ziehung","MaskedITN":"Wie ist Alina Ziehung","Display":"Wie ist Alina Ziehung?"}]}