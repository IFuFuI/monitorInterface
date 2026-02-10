
Public Class Def_Busq_Transacciones
    ''' <summary>Versión del archivo</summary>
    Public strVersion As String
    ''' <summary>Comentarios Ej. Se incluye la transacción de retiro.</summary>
    Public strComentarios As String
    ''' <summary>Nombre de la persona que actualiza el archivo.</summary>
    Public strModifiedBy As String
    ''' <summary>Si esta configurado este parametro el aplicativo utiliza el evento de windows de cambio de archivo .</summary>
    Public UseWindowsFileChange As String
    ''' <summary>Utiliza las cadenas de estatus de HW de NDC para reportar .</summary>
    Public bUseNDCHwStatus As String
    ''' <summary>Este parametro se utiliza cuando useWindowsFileChange esta seteado en false, es el tiempo que va a estar poleando el archivo.</summary>
    Public iTimeCheck As String
    ''' <summary>Si el nombre del archivo que se esta modificando contiene alguna de las cadenas que estan configuradas en la lista, no lo toma en cuenta.</summary>
    Public listFilesNotApplicables As New List(Of String)
    ''' <summary>Se configura el nombre del archivo, aplica solamente para cajeros que no dividan la journal en mas archivos</summary>
    Public oneJournalFile As String
    ''' <summary>Lista con la definicion de las transacciones a reportar.</summary>
    Public listTxnData As New List(Of Transaccion_Data)
    ''' <summary>Lista con la definicion de los estados de SW.</summary>
    Public listEstatusSw As New List(Of EstatuSW_Data)
    ''' <summary>Lista con la definicion de los estados de HW.</summary>
    Public listEstatusHw As New List(Of EstatuHW_Data)

    Public debug As String
    ''' <summary>Lista con la definicion de los estados de HW.</summary>
    ''' En casos donde las aplicaciones escriban un terminador al final de la journal (Banjercito)
    Public bRemoveLastLine As Boolean

End Class
Public Class Transaccion_Data
    Public sizeOfParamArray As String = ""
    Public defaultCharacterForEmptyValues As String = ""
    ''' <summary>Lista de posibles Palabras clave de inicio de transaccion</summary>
    Public listPalabrasI As New List(Of String)
    ''' <summary>Id asociado a la pizarra</summary>
    Public strId_Txn As String = "" '
    ''' <summary>Descripción de la txn - Unicamente informativo</summary>
    Public strDescr As String = ""
    ''' <summary>Si esta activa esta opción busca el texto en la extension</summary>
    Public UseWDExtension As String = ""
    ''' <summary>Si esta activa esta opción busca el texto en la extension</summary>
    Public UseReversoExt As String = ""
    ''' <summary>Lista de los posibles estatus con los que termina la transaccion</summary>
    Public listFinTxn As New List(Of Fin_Txn)
    ''' <summary>Lista de los posibles parametros en la transaccion</summary>
    Public listParametros As New List(Of Parametros_Txn)

    Public listCaracMandat As New List(Of String)

    Public useCustomasiedApiToGetParameters As String = ""

End Class

Public Class Fin_Txn
    ''' <summary>Palabra clave de fin de transaccion</summary>
    Public strPal_Cve_F As String = ""
    ''' <summary>Id asociado al estatus de la transaccion en la pizarra.</summary>
    Public strId_Status As String = ""
    ''' <summary>Solo Informativo - Descripción del estatus</summary>
    Public strId_Desc As String = ""
End Class

Public Class Parametros_Txn
    Public oRegularExpression As New RegularExpression
    ''' <summary>Palabra clave de inicio del parametro</summary>
    Public strP_I As String = ""
    ''' <summary>Palabra clave de fin del parametro</summary>
    Public strP_F As String = ""
    ''' <summary>Caracter obligatorio que debe contener el parametro</summary>
    Public listCaracMandat As New List(Of String)
    ''' <summary>Descripcion (informativo unicamente)</summary>
    Public strDescription As String = ""
    ''' <summary>Posicicion del arreglo donde se va a guardar</summary>
    Public strPos As String = ""
    ''' <summary>Caracteres que se van a remover si se encuentra el parametro</summary>
    Public listCaractRemover As New List(Of String)
End Class

Public Class RegularExpression
    Public bRemoveSpaces As String = ""
    Public Expression As String = ""
    Public Pos As String = ""
End Class

Public Class EstatuSW_Data
    Public listPC_Estatus As New List(Of String)
    Public strIdEstatus As String
    Public strDescripcion As String
    Public recoverDevices As String
    Public listEstatusHw As New List(Of EstatuHW_Data)
End Class

Public Class EstatuHW_Data
    Public listPC_Estatus As New List(Of String)
    Public strIdDisp As String
    Public strIdEstatus As String
    Public strIdEstatusExt As String
    Public strAlerta As String
    Public strDescripcion As String
    Public uM_or_S As String
    Public m_Data As String
    Public r_Posicion As String
    Public r_Valor As String
End Class