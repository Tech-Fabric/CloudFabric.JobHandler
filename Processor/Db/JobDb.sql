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

--
-- TOC entry 216 (class 1259 OID 16437)
-- Name: Job; Type: TABLE; Schema: public; Owner: cloudfabric_job_dev
--

CREATE TABLE public."Job" (
    "Id" uuid NOT NULL,
    "JobTypeId" integer NOT NULL,
    "Created" timestamp without time zone NOT NULL,
    "CreatorId" integer,
    "JobStatusId" integer NOT NULL,
    "Parameters" text,
    "TenantId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'::uuid
);


ALTER TABLE public."Job" OWNER TO cloudfabric_job_dev;

--
-- TOC entry 218 (class 1259 OID 16447)
-- Name: JobCompleted; Type: TABLE; Schema: public; Owner: cloudfabric_job_dev
--

CREATE TABLE public."JobCompleted" (
    "JobId" uuid NOT NULL,
    "JobStatusId" integer NOT NULL,
    "Completed" timestamp without time zone NOT NULL,
    "Id" uuid NOT NULL,
    "ErrorMessage" text
);


ALTER TABLE public."JobCompleted" OWNER TO cloudfabric_job_dev;

--
-- TOC entry 217 (class 1259 OID 16442)
-- Name: JobProcess; Type: TABLE; Schema: public; Owner: cloudfabric_job_dev
--

CREATE TABLE public."JobProcess" (
    "Id" uuid NOT NULL,
    "JobStatusId" integer NOT NULL,
    "Progress" integer NOT NULL,
    "LastProcess" timestamp without time zone NOT NULL,
    "JobId" uuid NOT NULL
);


ALTER TABLE public."JobProcess" OWNER TO cloudfabric_job_dev;

--
-- TOC entry 219 (class 1259 OID 16450)
-- Name: JobStatus; Type: TABLE; Schema: public; Owner: cloudfabric_job_dev
--

CREATE TABLE public."JobStatus" (
    "Id" integer NOT NULL,
    "Name" character varying(20) NOT NULL
);


ALTER TABLE public."JobStatus" OWNER TO cloudfabric_job_dev;

--
-- TOC entry 215 (class 1259 OID 16429)
-- Name: JobType; Type: TABLE; Schema: public; Owner: cloudfabric_job_dev
--

CREATE TABLE public."JobType" (
    "Id" integer NOT NULL,
    "Name" character varying(20) NOT NULL,
    "AssemblyName" character varying(2000) NOT NULL
);


ALTER TABLE public."JobType" OWNER TO cloudfabric_job_dev;

--
-- TOC entry 214 (class 1259 OID 16428)
-- Name: TaskType_Id_seq; Type: SEQUENCE; Schema: public; Owner: cloudfabric_job_dev
--

CREATE SEQUENCE public."TaskType_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public."TaskType_Id_seq" OWNER TO cloudfabric_job_dev;

--
-- TOC entry 3624 (class 0 OID 0)
-- Dependencies: 214
-- Name: TaskType_Id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: cloudfabric_job_dev
--

ALTER SEQUENCE public."TaskType_Id_seq" OWNED BY public."JobType"."Id";


--
-- TOC entry 3455 (class 2604 OID 16432)
-- Name: JobType Id; Type: DEFAULT; Schema: public; Owner: cloudfabric_job_dev
--

ALTER TABLE ONLY public."JobType" ALTER COLUMN "Id" SET DEFAULT nextval('public."TaskType_Id_seq"'::regclass);


--
-- TOC entry 3618 (class 0 OID 16450)
-- Dependencies: 219
-- Data for Name: JobStatus; Type: TABLE DATA; Schema: public; Owner: cloudfabric_job_dev
--

COPY public."JobStatus" ("Id", "Name") FROM stdin;
0	Ready
10	InProgress
30	Success
40	Failed
\.


--
-- TOC entry 3614 (class 0 OID 16429)
-- Dependencies: 215
-- Data for Name: JobType; Type: TABLE DATA; Schema: public; Owner: cloudfabric_job_dev
--

COPY public."JobType" ("Id", "Name", "AssemblyName") FROM stdin;
1	RebuildIndex	
\.


--
-- TOC entry 3625 (class 0 OID 0)
-- Dependencies: 214
-- Name: TaskType_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: cloudfabric_job_dev
--

SELECT pg_catalog.setval('public."TaskType_Id_seq"', 1, false);


--
-- PostgreSQL database dump complete
--

