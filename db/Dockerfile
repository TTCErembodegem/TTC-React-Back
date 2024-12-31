FROM mysql:5.5.60

# Attempt to fix sql scripts not running on Synology
# This did not fix the issue...
COPY *.sql /docker-entrypoint-initdb.d/
RUN chmod 644 /docker-entrypoint-initdb.d/*.sql

CMD ["mysqld"]
