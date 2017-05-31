package nz.geek.nathan.minesweeper.base

import android.app.Activity
import android.os.Bundle
import android.support.v7.app.AppCompatActivity
import javax.inject.Inject

/**
 * Created by nate on 17/05/17.
 */
abstract class BaseActivity<P : BaseContract.Presenter>: AppCompatActivity() {
    @Inject internal lateinit var _presenter: P

    val mPresenter: P
        get() = _presenter


    protected abstract fun setupActivityComponent()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setupActivityComponent()
    }

    override fun onPause(){
        super.onPause()
        mPresenter.stop()
    }

    override fun onResume() {
        super.onResume()
        mPresenter.start()
    }
}
