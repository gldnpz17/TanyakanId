import c from "./not-found.module.css";

const NotFoundPage = () => (
    <div className={c.container}>
        <p>
            Maaf, halaman yang Anda tuju tidak ditemukan.
            Mohon periksa kembali pengetikan alamat URL.
        </p>
    </div>
);

export default NotFoundPage;