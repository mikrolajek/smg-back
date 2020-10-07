CREATE TABLE public.location (
    id integer NOT NULL,
    id_company integer NOT NULL,
    address text NOT NULL
);
CREATE SEQUENCE public.branch_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
ALTER SEQUENCE public.branch_id_seq OWNED BY public.location.id;
CREATE TABLE public.code (
    id integer NOT NULL,
    type text NOT NULL,
    uid text NOT NULL
);
CREATE SEQUENCE public.code_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
ALTER SEQUENCE public.code_id_seq OWNED BY public.code.id;
CREATE TABLE public.company (
    id integer NOT NULL,
    name text NOT NULL
);
CREATE SEQUENCE public.company_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
ALTER SEQUENCE public.company_id_seq OWNED BY public.company.id;
CREATE TABLE public."group" (
    id integer NOT NULL,
    id_product integer NOT NULL,
    id_code integer NOT NULL,
    id_link integer NOT NULL,
    from_date date DEFAULT now() NOT NULL,
    to_date date,
    id_location integer NOT NULL
);
CREATE SEQUENCE public.group_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
ALTER SEQUENCE public.group_id_seq OWNED BY public."group".id;
CREATE TABLE public.link (
    id integer NOT NULL,
    url text NOT NULL
);
CREATE TABLE public.product (
    id integer NOT NULL,
    name text NOT NULL
);
CREATE SEQUENCE public.product_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
ALTER SEQUENCE public.product_id_seq OWNED BY public.product.id;
CREATE SEQUENCE public.url_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
ALTER SEQUENCE public.url_id_seq OWNED BY public.link.id;
ALTER TABLE ONLY public.code ALTER COLUMN id SET DEFAULT nextval('public.code_id_seq'::regclass);
ALTER TABLE ONLY public.company ALTER COLUMN id SET DEFAULT nextval('public.company_id_seq'::regclass);
ALTER TABLE ONLY public."group" ALTER COLUMN id SET DEFAULT nextval('public.group_id_seq'::regclass);
ALTER TABLE ONLY public.link ALTER COLUMN id SET DEFAULT nextval('public.url_id_seq'::regclass);
ALTER TABLE ONLY public.location ALTER COLUMN id SET DEFAULT nextval('public.branch_id_seq'::regclass);
ALTER TABLE ONLY public.product ALTER COLUMN id SET DEFAULT nextval('public.product_id_seq'::regclass);
ALTER TABLE ONLY public.location
    ADD CONSTRAINT branch_pkey PRIMARY KEY (id);
ALTER TABLE ONLY public.code
    ADD CONSTRAINT code_pkey PRIMARY KEY (id);
ALTER TABLE ONLY public.code
    ADD CONSTRAINT code_uid_key UNIQUE (uid);
ALTER TABLE ONLY public.company
    ADD CONSTRAINT company_pkey PRIMARY KEY (id);
ALTER TABLE ONLY public."group"
    ADD CONSTRAINT group_pkey PRIMARY KEY (id);
ALTER TABLE ONLY public.product
    ADD CONSTRAINT product_pkey PRIMARY KEY (id);
ALTER TABLE ONLY public.link
    ADD CONSTRAINT url_pkey PRIMARY KEY (id);
ALTER TABLE ONLY public.location
    ADD CONSTRAINT branch_id_company_fkey FOREIGN KEY (id_company) REFERENCES public.company(id) ON UPDATE RESTRICT ON DELETE RESTRICT;
ALTER TABLE ONLY public."group"
    ADD CONSTRAINT group_id_code_fkey FOREIGN KEY (id_code) REFERENCES public.code(id) ON UPDATE RESTRICT ON DELETE RESTRICT;
ALTER TABLE ONLY public."group"
    ADD CONSTRAINT group_id_link_fkey FOREIGN KEY (id_link) REFERENCES public.link(id) ON UPDATE RESTRICT ON DELETE RESTRICT;
ALTER TABLE ONLY public."group"
    ADD CONSTRAINT group_id_location_fkey FOREIGN KEY (id_location) REFERENCES public.location(id) ON UPDATE RESTRICT ON DELETE RESTRICT;
ALTER TABLE ONLY public."group"
    ADD CONSTRAINT group_id_product_fkey FOREIGN KEY (id_product) REFERENCES public.product(id) ON UPDATE RESTRICT ON DELETE RESTRICT;
