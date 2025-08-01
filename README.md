# CompartirTXT - API .NET Core 8

Proyecto que implementa una API REST en .NET Core 8 con los siguientes endpoints:

- `POST /api/texto`: Guarda texto en un archivo.
- `GET /api/texto`: Recupera el texto guardado.

El archivo se guarda en una carpeta compartida a través de un servidor Samba/NFS, y la aplicación está desplegada en **IIS**.

## 📁 Ruta del archivo compartido

`\\192.168.1.9\share\texto.txt`

## ⚙️ Tecnologías

- .NET Core 8
- C#
- IIS (Servidor local)
- SMB/Samba (File Server)
- HTML para frontend básico

#
---

👤 **Autor**:  (@jonathanc4chan)  
📧 jchanc4@miumg.edu.gt
