--
-- PostgreSQL database dump
--

-- Dumped from database version 15.2
-- Dumped by pg_dump version 15.2

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

CREATE TABLE public."Job" (
    Id uuid NOT NULL PRIMARY KEY,
    JobTypeId integer NOT NULL,
    Created timestamp without time zone NOT NULL,
    CreatorId integer,
    JobStatusId integer NOT NULL,
    Parameters text,
    TenantId uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'::uuid
);


ALTER TABLE public."Job" OWNER TO cloudfabric_job_dev;

CREATE TABLE public."JobCompleted" (
    Id uuid NOT NULL PRIMARY KEY,
    JobId uuid NOT NULL,
    JobStatusId integer NOT NULL,
    Completed timestamp without time zone NOT NULL,
    ErrorMessage text
);


ALTER TABLE public."JobCompleted" OWNER TO cloudfabric_job_dev;

CREATE TABLE public."JobProcess" (
    Id uuid NOT NULL PRIMARY KEY,
    JobStatusId integer NOT NULL,
    Progress integer NOT NULL,
    LastProcess timestamp without time zone NOT NULL,
    JobId uuid NOT NULL
);


ALTER TABLE public."JobProcess" OWNER TO cloudfabric_job_dev;

CREATE TABLE public."JobStatus" (
    Id integer NOT NULL PRIMARY KEY,
    Name character varying(20) NOT NULL
);


ALTER TABLE public."JobStatus" OWNER TO cloudfabric_job_dev;

CREATE TABLE public."JobType" (
    Id integer NOT NULL PRIMARY KEY,
    Name character varying(20) NOT NULL,
    AssemblyName character varying(2000) NULL
);

ALTER TABLE public."JobType" OWNER TO cloudfabric_job_dev;

INSERT INTO public."JobStatus" (Id, Name) VALUES
(0,	'Ready'),
(10,	'InProgress'),
(30,	'Success'),
(40,	'Failed');


INSERT INTO public."JobType"(Id, Name, AssemblyName) VALUES (1, 'RebuildIndex', '');

--
-- PostgreSQL database dump complete
--
