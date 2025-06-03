
# Kinetic Challenge

Este repositorio incluye:

- API `Kinetic.Inventory.API`
- Servicio `Kinetic.Notification.Service`
- Proyecto de test `Kinetic.Notification.Tests`

## Arquitectura:

([Productor] API `Kinetic.Inventory.API` ) --> [RabbitMQ] --> ([Consumidor] Worker Service `Kinetic.Notification.Service`)

## ▶️ Cómo ejecutar el proyecto con Docker

1. Posicionarse en la carpeta raíz del proyecto, donde se encuentra el archivo docker-compose.yaml

2. Abrir una linea de comandos y ejecutar: docker compose up -d

3. Verificar que se hayan creado correctamnte los 3 contenedores agrupados.

4. Ir a http://localhost:15672/ y loguearse ocn las credenciales: user/password

5. Ir a "Exchanges" y crear un nuevo exchange llamado "inventory_exchange", del tipo "Direct" y "Transient"

6. Ir a pestaña "Queues and Streams" y crear 3 queues en "Add a new queue", llamadas "create", "update" y "delete"

7. Ir a "Exchanges", clickear en el nuevo exchange creado, 
	y luego ir a "Add binding from this exchange", y agregar 3 binding,. uno para cada queue,
	ejemplo, para la queue create, En "To queue" escribir "create" y en "Routing key" escribir "create", 
	lo mismo para "update" y "delete"

8. Verificar que el servicio Notification.Service ahora se puede conectar sin problemas

9. ir a http://localhost:5000/swagger/index.html, para poder testear el API, donde tendremos métodos expuestos, 
y podemos verificar el funcionamiento de la solución con ellos.


