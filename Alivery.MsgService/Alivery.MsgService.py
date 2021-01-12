#!/usr/bin/env python
import pika


cred = pika.PlainCredentials('test','test')

connection = pika.BlockingConnection(pika.ConnectionParameters('185-167-96-77.cloud-xip.io',5672,
                                       '/', credentials = cred))
channel = connection.channel()

ch=channel.queue_declare(queue='hello')

channel.basic_publish(exchange='',
                      routing_key='hello',
                      body='Hello World!')
print(" [x] Sent 'Hello World!'")